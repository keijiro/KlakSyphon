#import <Foundation/Foundation.h>
#import "IUnityGraphicsMetal.h"
#import "Syphon/LiteServer.h"

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
