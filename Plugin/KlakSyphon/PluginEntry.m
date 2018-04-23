#import <Metal/MTLTexture.h>
#import "SyphonServerDirectory.h"
#import "Syphon/LiteServer.h"
#import "Syphon/LiteClient.h"
#import "IUnityGraphicsMetal.h"

#pragma mark Device interface retrieval

static IUnityInterfaces* s_interfaces;
static IUnityGraphicsMetal* s_graphics;

static id<MTLDevice> GetMetalDevice()
{
    if (!s_graphics) s_graphics = UNITY_GET_INTERFACE(s_interfaces, IUnityGraphicsMetal);
    return s_graphics ? s_graphics->MetalDevice() : nil;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* interfaces)
{
    s_interfaces = interfaces;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload(void)
{
    s_interfaces = NULL;
    s_graphics = NULL;
}

#pragma mark - Plugin server functions

LiteServer* Klak_CreateServer(const char* cname, int width, int height)
{
    NSString* name = [NSString stringWithUTF8String:cname];
    NSLog(@"KlakSyphon: CreateServer (%@, %d, %d)", name, width, height);
    return [[LiteServer alloc] initWithName:name
                                 dimensions:NSMakeSize(width, height)
                                     device:GetMetalDevice()];
}

void Klak_DestroyServer(LiteServer* server)
{
    NSLog(@"KlakSyphon: DestroyServer (%p)", server);
    [server release];
}

void* Klak_GetServerTexture(LiteServer* server)
{
    return server.texture;
}

void Klak_PublishServerTexture(LiteServer* server)
{
    [server publishNewFrame];
}

#pragma mark - Plugin client functions

void* Klak_CreateClient(void)
{
    NSArray* servers = [[SyphonServerDirectory sharedDirectory] servers];
    if (servers.count == 0) return NULL;
    return [[LiteClient alloc] initWithServerDescription:servers[0]];
}

void Klak_DestroyClient(LiteClient* client)
{
    [client release];
}

void* Klak_GetClientTexture(LiteClient* client)
{
    return client.texture;
}

int Klak_GetClientTextureWidth(LiteClient* client)
{
    return (int)IOSurfaceGetWidth(client.texture.iosurface);
}

int Klak_GetClientTextureHeight(LiteClient* client)
{
    return (int)IOSurfaceGetHeight(client.texture.iosurface);
}

static void ClientUpdateCallback(int eventID, void* data)
{
    LiteClient* client = data;
    [client updateFromRenderThread:GetMetalDevice()];
}

void* Klak_GetClientUpdateCallback(void)
{
    return ClientUpdateCallback;
}
