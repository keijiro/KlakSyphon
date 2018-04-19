#import "LiteServer.h"
#import "SyphonPrivate.h"
#import "SyphonServerConnectionManager.h"
#import <IOSurface/IOSurface.h>
#import <OpenGL/gl3.h>

@interface LiteServer()
{
    NSString *_name;
    NSString *_uuid;
    NSSize _size;
    GLint _texture;
    GLuint _texture2;
    SyphonServerConnectionManager *_connection;
    IOSurfaceRef _ioSurface;
}

@property (readonly, retain) NSString* name;
@property (readonly) NSDictionary* description;

+ (void)retireRemainingServers;

@end

__attribute__((destructor))
static void finalizer()
{
    [LiteServer retireRemainingServers];
}

@implementation LiteServer

- (id)init
{
    return [self initWithName:nil dimensions:NSZeroSize textureName:0];
}

- (id)initWithName:(NSString*)name dimensions:(NSSize)size textureName:(int)texture
{
    if (self = [super init])
    {
        _name = [name copy];
        _uuid = SyphonCreateUUIDString();
        _size = size;
        _texture = texture;

        _connection = [[SyphonServerConnectionManager alloc] initWithUUID:_uuid options:nil];
        if (![_connection start])
        {
            [self release];
            return nil;
        }
        
        [[self class] addServerToRetireList:_uuid];
        [self startBroadcasts];        
    }
    return self;
}

- (void) shutDownServer
{
    if (_connection)
    {
        [_connection stop];
        [_connection release];
        _connection = nil;
    }
    
    [self destroyIOSurface];

    [self stopBroadcasts];
    [[self class] removeServerFromRetireList:_uuid];
}

- (void) dealloc
{
    [self shutDownServer];
    [_name release];
    [_uuid release];
    [super dealloc];
}

- (NSDictionary*)description
{
    NSDictionary *surface = _connection.surfaceDescription;
    if (!surface) surface = [NSDictionary dictionary];
    /*
     Getting the app name: helper tasks, command-line tools, etc, don't have a NSRunningApplication instance,
     so fall back to NSProcessInfo in those cases, then use an empty string as a last resort.
     
     http://developer.apple.com/library/mac/qa/qa1544/_index.html
     
     */
    NSString *appName = [[NSRunningApplication currentApplication] localizedName];
    if (!appName) appName = [[NSProcessInfo processInfo] processName];
    if (!appName) appName = [NSString string];
    
    return [NSDictionary dictionaryWithObjectsAndKeys:
            [NSNumber numberWithUnsignedInt:kSyphonDictionaryVersion], SyphonServerDescriptionDictionaryVersionKey,
            self.name, SyphonServerDescriptionNameKey,
            _uuid, SyphonServerDescriptionUUIDKey,
            appName, SyphonServerDescriptionAppNameKey,
            [NSArray arrayWithObject:surface], SyphonServerDescriptionSurfacesKey,
            nil];
}

- (void)stop
{
    [self shutDownServer];
}

- (void)updateFromRenderThread
{
    if (!_ioSurface)
    {
        [self setupIOSurfaceForSize];
        [_connection setSurfaceID:IOSurfaceGetID(_ioSurface)];
    }
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_RECTANGLE, _texture2, 0);
    [_connection publishNewFrame];
}

- (void)unbound
{
}

#pragma mark -
#pragma mark Private methods

#pragma mark FBO & IOSurface handling

- (void) setupIOSurfaceForSize
{
    // init our texture and IOSurface
    NSDictionary* surfaceAttributes = [[NSDictionary alloc] initWithObjectsAndKeys:[NSNumber numberWithBool:YES], (NSString*)kIOSurfaceIsGlobal,
                                       [NSNumber numberWithUnsignedInteger:(NSUInteger)_size.width], (NSString*)kIOSurfaceWidth,
                                       [NSNumber numberWithUnsignedInteger:(NSUInteger)_size.height], (NSString*)kIOSurfaceHeight,
                                       [NSNumber numberWithUnsignedInteger:4U], (NSString*)kIOSurfaceBytesPerElement, nil];
    
    _ioSurface =  IOSurfaceCreate((CFDictionaryRef) surfaceAttributes);
    [surfaceAttributes release];
    
    glGenTextures(1, &_texture2);
    glBindTexture(GL_TEXTURE_RECTANGLE, _texture2);
    CGLTexImageIOSurface2D(CGLGetCurrentContext(), GL_TEXTURE_RECTANGLE, GL_RGBA8, _size.width, _size.height, GL_BGRA, GL_UNSIGNED_INT_8_8_8_8_REV, _ioSurface, 0);
}

- (void) destroyIOSurface
{
    if (!_ioSurface)
    {
        CFRelease(_ioSurface);
        _ioSurface = NULL;
    }
}

#pragma mark Notification Handling for Server Presence

/*
 Broadcast and discovery is done via NSDistributedNotificationCenter. Servers notify announce, change (currently only affects name) and retirement.
 Discovery is done by a discovery-request notification, to which servers respond with an announce.
 
 If this gets unweildy we could move it into a SyphonBroadcaster class
 */

/*
 We track all instances and send a retirement broadcast for any which haven't been stopped when the code is unloaded.
 */

static OSSpinLock mRetireListLock = OS_SPINLOCK_INIT;
static NSMutableSet *mRetireList = nil;

+ (void)addServerToRetireList:(NSString *)serverUUID
{
    OSSpinLockLock(&mRetireListLock);
    if (mRetireList == nil)
    {
        mRetireList = [[NSMutableSet alloc] initWithCapacity:1U];
    }
    [mRetireList addObject:serverUUID];
    OSSpinLockUnlock(&mRetireListLock);
}

+ (void)removeServerFromRetireList:(NSString *)serverUUID
{
    OSSpinLockLock(&mRetireListLock);
    [mRetireList removeObject:serverUUID];
    if ([mRetireList count] == 0)
    {
        [mRetireList release];
        mRetireList = nil;
    }
    OSSpinLockUnlock(&mRetireListLock);
}

+ (void)retireRemainingServers
{
    // take the set out of the global so we don't hold the spin-lock while we send the notifications
    // even though there should never be contention for this
    NSMutableSet *mySet = nil;
    OSSpinLockLock(&mRetireListLock);
    mySet = mRetireList;
    mRetireList = nil;
    OSSpinLockUnlock(&mRetireListLock);
    for (NSString *uuid in mySet) {
        SYPHONLOG(@"Retiring a server at code unload time because it was not properly stopped");
        NSDictionary *fakeServerDescription = [NSDictionary dictionaryWithObject:uuid forKey:SyphonServerDescriptionUUIDKey];
        [[NSDistributedNotificationCenter defaultCenter] postNotificationName:SyphonServerRetire
                                                                       object:SyphonServerDescriptionUUIDKey
                                                                     userInfo:fakeServerDescription
                                                           deliverImmediately:YES];
    }
    [mySet release];
}

- (void)startBroadcasts
{
    // Register for any Announcement Requests.
    [[NSDistributedNotificationCenter defaultCenter] addObserver:self selector:@selector(handleDiscoveryRequest:) name:SyphonServerAnnounceRequest object:nil];
    
    [self broadcastServerAnnounce];
}

- (void) handleDiscoveryRequest:(NSNotification*) aNotification
{
    SYPHONLOG(@"Got Discovery Request");
    
    [self broadcastServerAnnounce];
}

- (void)broadcastServerAnnounce
{
    NSDictionary *description = self.description;
    [[NSDistributedNotificationCenter defaultCenter] postNotificationName:SyphonServerAnnounce
                                                                   object:[description objectForKey:SyphonServerDescriptionUUIDKey]
                                                                 userInfo:description
                                                       deliverImmediately:YES];
}

- (void)broadcastServerUpdate
{
    NSDictionary *description = self.description;
    [[NSDistributedNotificationCenter defaultCenter] postNotificationName:SyphonServerUpdate
                                                                   object:[description objectForKey:SyphonServerDescriptionUUIDKey]
                                                                 userInfo:description
                                                       deliverImmediately:YES];
}

- (void)stopBroadcasts
{
    [[NSDistributedNotificationCenter defaultCenter] removeObserver:self];
    NSDictionary *description = self.description;
    [[NSDistributedNotificationCenter defaultCenter] postNotificationName:SyphonServerRetire
                                                                   object:[description objectForKey:SyphonServerDescriptionUUIDKey]
                                                                 userInfo:description
                                                       deliverImmediately:YES];
}

@end
