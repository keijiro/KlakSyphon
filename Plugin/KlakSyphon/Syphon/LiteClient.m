#import "LiteClient.h"
#import "SyphonClientConnectionManager.h"
#import <Metal/MTLDevice.h>
#import <Metal/MTLTexture.h>

@interface LiteClient() <SyphonInfoReceiving>
{
    SyphonClientConnectionManager *_connection;
    id <MTLTexture> _texture;
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
        if (_connection == nil)
        {
            [self release];
            return nil;
        }
        [_connection addInfoClient:self isFrameClient:NO];
    }
    return self;
}

- (void)dealloc
{
    [_connection removeInfoClient:self isFrameClient:NO];
    [_connection release];
    if (_texture) [_texture release];
    [super dealloc];
}

- (BOOL)isValid
{
    return _connection.isValid;
}

- (void)invalidateFrame
{
}

- (void)updateWithDevice:(id<MTLDevice>)device
{
    IOSurfaceRef surface = [_connection surfaceHavingLock];

    if (_texture)
    {
        if (_texture.iosurface != surface)
        {
            [_texture release];
            _texture = nil;
        }
    }
    
    if (!_texture && surface)
    {
        MTLTextureDescriptor* desc = [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:MTLPixelFormatBGRA8Unorm
                                                                                        width:IOSurfaceGetWidth(surface)
                                                                                       height:IOSurfaceGetHeight(surface)
                                                                                    mipmapped:NO];
        _texture = [device newTextureWithDescriptor:desc iosurface:surface plane:0];
    }
}

@end
