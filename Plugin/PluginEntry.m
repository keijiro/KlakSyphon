// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

// Unity native plugin entry points

#import <Metal/MTLTexture.h>
#import "SyphonServerDirectory.h"
#import "LiteServer.h"
#import "LiteClient.h"
#import "IUnityGraphicsMetal.h"

#pragma mark Device interface retrieval

static IUnityInterfaces *s_interfaces;
static IUnityGraphicsMetalV1 *s_graphics;

static id <MTLDevice> GetMetalDevice()
{
    if (!s_graphics) s_graphics = UNITY_GET_INTERFACE(s_interfaces, IUnityGraphicsMetalV1);
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

#pragma mark - Plugin common functions

static MTLPixelFormat s_pixelFormat = MTLPixelFormatBGRA8Unorm;

void Plugin_EnableColorSpaceConversion(void)
{
    s_pixelFormat = MTLPixelFormatBGRA8Unorm_sRGB;
}

void Plugin_DisableColorSpaceConversion(void)
{
    s_pixelFormat = MTLPixelFormatBGRA8Unorm;
}

#pragma mark - Plugin server functions

LiteServer *Plugin_CreateServer(const char *cname, int width, int height)
{
    NSString *name = [NSString stringWithUTF8String:cname];
    return [[LiteServer alloc] initWithName:name
                                 dimensions:NSMakeSize(width, height)
                                pixelFormat:s_pixelFormat
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

void *Plugin_CreateClient(const char *pName, const char *pAppName)
{
    NSString *name = (pName == NULL) ? nil : [NSString stringWithUTF8String:pName];
    NSString *appName = (pAppName == NULL) ? nil : [NSString stringWithUTF8String:pAppName];
    SyphonServerDirectory *dir = SyphonServerDirectory.sharedDirectory;
    NSArray *servers = [dir serversMatchingName:name appName:appName];
    if (servers.count == 0) return NULL;
    return [[LiteClient alloc] initWithServerDescription:servers[0]];
}

void Plugin_DestroyClient(LiteClient *client)
{
    [client release];
}

int Plugin_IsClientValid(LiteClient *client)
{
    return client.isValid ? 1 : 0;
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

void Plugin_UpdateClient(LiteClient *client)
{
    [client updateWithDevice:GetMetalDevice() pixelFormat:s_pixelFormat];
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
