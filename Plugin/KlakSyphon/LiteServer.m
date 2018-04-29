// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

// LiteServer: Simplified implementation of Syphon server

#import "LiteServer.h"
#import "SyphonServerConnectionManager.h"
#import <Metal/MTLDevice.h>
#import <Metal/MTLTexture.h>

@interface LiteServer()
{
    NSString *_name;
    NSString *_uuid;
    IOSurfaceRef _ioSurface;
    id <MTLTexture> _texture;
    SyphonServerConnectionManager *_connection;
}

@property (readonly) NSDictionary *description;

@end

@implementation LiteServer

- (id)init
{
    [self doesNotRecognizeSelector:_cmd];
    return nil;
}

- (id)initWithName:(NSString *)name dimensions:(NSSize)size pixelFormat:(MTLPixelFormat)format device:(id <MTLDevice>)device
{
    if (self = [super init])
    {
        _name = [name copy];
        _uuid = SyphonCreateUUIDString();

        NSDictionary *attribs = @{ (NSString *)kIOSurfaceIsGlobal: @YES,
                                   (NSString *)kIOSurfaceWidth: @(size.width),
                                   (NSString *)kIOSurfaceHeight: @(size.height),
                                   (NSString *)kIOSurfaceBytesPerElement: @4u };
        _ioSurface = IOSurfaceCreate((CFDictionaryRef)attribs);

        MTLTextureDescriptor *desc = [MTLTextureDescriptor texture2DDescriptorWithPixelFormat:format
                                                                                        width:size.width
                                                                                       height:size.height
                                                                                    mipmapped:NO];
        _texture = [device newTextureWithDescriptor:desc iosurface:_ioSurface plane:0];

        _connection = [[SyphonServerConnectionManager alloc] initWithUUID:_uuid options:nil];
        [_connection setSurfaceID:IOSurfaceGetID(_ioSurface)];
        [_connection start];

        [self startBroadcasts];
    }
    return self;
}

- (void)dealloc
{
    [self stopBroadcasts];

    if (_connection)
    {
        [_connection stop];
        [_connection release];
    }

    [_texture release];
    CFRelease(_ioSurface);

    [_name release];
    [_uuid release];

    [super dealloc];
}

#pragma mark - Public method

- (void)publishNewFrame
{
    [_connection publishNewFrame];
}

#pragma mark - Private method

- (NSDictionary *)description
{
    NSDictionary *surface = _connection.surfaceDescription;
    if (!surface) surface = [NSDictionary dictionary];

    // Getting the app name: helper tasks, command-line tools, etc, don't have a NSRunningApplication instance,
    // so fall back to NSProcessInfo in those cases, then use an empty string as a last resort.
    // http://developer.apple.com/library/mac/qa/qa1544/_index.html
    NSString *appName = [[NSRunningApplication currentApplication] localizedName];
    if (!appName) appName = [[NSProcessInfo processInfo] processName];
    if (!appName) appName = [NSString string];

    return @{ SyphonServerDescriptionDictionaryVersionKey: @kSyphonDictionaryVersion,
              SyphonServerDescriptionNameKey: _name,
              SyphonServerDescriptionUUIDKey: _uuid,
              SyphonServerDescriptionAppNameKey: appName,
              SyphonServerDescriptionSurfacesKey: @[ surface ] };
}

#pragma mark - Notification handling

- (void)startBroadcasts
{
    [NSDistributedNotificationCenter.defaultCenter addObserver:self
                                                      selector:@selector(handleDiscoveryRequest:)
                                                          name:SyphonServerAnnounceRequest
                                                        object:nil];
    [self postNotification:SyphonServerAnnounce];
}

- (void)stopBroadcasts
{
    [NSDistributedNotificationCenter.defaultCenter removeObserver:self];
    [self postNotification:SyphonServerRetire];
}

- (void)handleDiscoveryRequest:(NSNotification *)aNotification
{
    [self postNotification:SyphonServerAnnounce];
}

- (void)postNotification:(NSString *)notificationName
{
    NSDictionary *description = self.description;
    [NSDistributedNotificationCenter.defaultCenter postNotificationName:notificationName
                                                                 object:description[SyphonServerDescriptionUUIDKey]
                                                               userInfo:description
                                                     deliverImmediately:YES];
}

@end
