#import <Metal/MTLTexture.h>
#import "SyphonServerDirectory.h"
#import "Syphon/LiteServer.h"
#import "Syphon/LiteClient.h"
#import "IUnityGraphicsMetal.h"

#pragma mark Device interface retrieval

static IUnityInterfaces *s_interfaces;
static IUnityGraphicsMetal *s_graphics;

static id <MTLDevice> GetMetalDevice()
{
    if (!s_graphics) s_graphics = UNITY_GET_INTERFACE(s_interfaces, IUnityGraphicsMetal);
    return s_graphics ? s_graphics->MetalDevice() : nil;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces *interfaces)
{
    s_interfaces = interfaces;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload(void)
{
    s_interfaces = NULL;
    s_graphics = NULL;
}

#pragma mark - Plugin server functions

LiteServer *Plugin_CreateServer(const char *cname, int width, int height)
{
    NSString *name = [NSString stringWithUTF8String:cname];
    return [[LiteServer alloc] initWithName:name
                                 dimensions:NSMakeSize(width, height)
                                     device:GetMetalDevice()];
}

void Plugin_DestroyServer(LiteServer *server)
{
    [server release];
}

void *Plugin_GetServerTexture(LiteServer *server)
{
    return server.texture;
}

void Plugin_PublishServerTexture(LiteServer *server)
{
    [server publishNewFrame];
}

#pragma mark - Plugin client functions

void *Plugin_CreateClient(const char *name, const char *appName)
{
    SyphonServerDirectory *dir = SyphonServerDirectory.sharedDirectory;
    NSArray *servers = [dir serversMatchingName:[NSString stringWithUTF8String:name]
                                        appName:[NSString stringWithUTF8String:appName]];
    if (servers.count == 0) return NULL;
    return [[LiteClient alloc] initWithServerDescription:servers[0]];
}

void Plugin_DestroyClient(LiteClient *client)
{
    [client release];
}

void *Plugin_GetClientTexture(LiteClient *client)
{
    return client.texture;
}

int Plugin_GetClientTextureWidth(LiteClient *client)
{
    return (int)IOSurfaceGetWidth(client.texture.iosurface);
}

int Plugin_GetClientTextureHeight(LiteClient *client)
{
    return (int)IOSurfaceGetHeight(client.texture.iosurface);
}

static void ClientUpdateCallback(int eventID, void *data)
{
    LiteClient *client = data;
    [client updateFromRenderThread:GetMetalDevice()];
}

void *Plugin_GetClientUpdateCallback(void)
{
    return ClientUpdateCallback;
}

#pragma mark - Plugin server directory functions

NSArray *Plugin_CreateServerList()
{
    return [SyphonServerDirectory.sharedDirectory.servers retain];
}

void Plugin_DestroyServerList(NSArray *list)
{
    [list release];
}

int Plugin_GetServerListCount(NSArray *list)
{
    return (int)list.count;
}

const void *Plugin_GetNameFromServerList(NSArray *list, int index)
{
    NSString *name = list[index][SyphonServerDescriptionNameKey];
    return name && name.length > 0 ? name.UTF8String : NULL;
}

const void *Plugin_GetAppNameFromServerList(NSArray *list, int index)
{
    NSString *name = list[index][SyphonServerDescriptionAppNameKey];
    return name && name.length > 0 ? name.UTF8String : NULL;
}
