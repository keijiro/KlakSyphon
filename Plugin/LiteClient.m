// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

// LiteClient: Simplified implementation of Syphon client

#import "LiteClient.h"
#import "SyphonClientConnectionManager.h"
#import <Metal/MTLDevice.h>
#import <Metal/MTLTexture.h>
#import <stdatomic.h>

@interface LiteClient() <SyphonInfoReceiving>
{
    SyphonClientConnectionManager *_connection;
    id <MTLTexture> _texture;
    atomic_bool _frameValid;
}
@end

@implementation LiteClient

- (id)init
{
    [self doesNotRecognizeSelector:_cmd];
    return nil;
}

- (id)initWithServerDescription:(NSDictionary *)description
{
    if (self = [super init])
    {
        _connection = [[SyphonClientConnectionManager alloc] initWithServerDescription:description];
        if (_connection == nil) return nil;
        [_connection addInfoClient:self isFrameClient:NO];
        atomic_store(&_frameValid, false);
    }
    return self;
}

- (void)dealloc
{
    [_connection removeInfoClient:self isFrameClient:NO];
}

- (BOOL)isValid
{
    return _connection.isValid;
}

- (void)invalidateFrame
{
    atomic_store(&_frameValid, false);
}

- (void)updateWithDevice:(id<MTLDevice>)device pixelFormat:(MTLPixelFormat)format
{
    if (atomic_load(&_frameValid)) return;

    _texture = nil;

    IOSurfaceRef surface = [_connection newSurface];
    if (surface != nil)
    {
        MTLTextureDescriptor* desc = [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:format
                                                                                        width:IOSurfaceGetWidth(surface)
                                                                                       height:IOSurfaceGetHeight(surface)
                                                                                    mipmapped:NO];
        _texture = [device newTextureWithDescriptor:desc iosurface:surface plane:0];
        CFRelease(surface);
    }

    atomic_store(&_frameValid, true);
}

@end
