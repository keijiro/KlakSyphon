#import <Foundation/Foundation.h>
#import "Unity/IUnityRenderingExtensions.h"
#import "Syphon/LiteServer.h"

static void PluginCallback(UnityRenderingExtEventType event, void* data)
{
//    NSLog(@"KlakSyphon: Plugin event (%d, %p)", event, data);
    if (event == 0)
        [(LiteServer*)data updateFromRenderThread];
    else
        [(LiteServer*)data unbound];
}

void* Klak_GetCallback()
{
    return PluginCallback;
}

LiteServer* Klak_CreateServer(const char* cname, int width, int height, void* texture)
{
    NSString* name = [NSString stringWithUTF8String:cname];
    NSLog(@"KlakSyphon: CreateServer (%@, %d, %d, %p)", name, width, height, texture);
    LiteServer* instance = [[LiteServer alloc] initWithName:name
                                                 dimensions:NSMakeSize(width, height)
                                                textureName:(int)texture];
    return instance;
}

void Klak_DestroyServer(LiteServer* server)
{
    NSLog(@"KlakSyphon: DestroyServer (%p)", server);
    [server release];
}
