#import <Foundation/Foundation.h>

@interface LiteServer : NSObject

- (id)initWithName:(NSString*)name dimensions:(NSSize)size textureName:(int)texture;
- (void)updateFromRenderThread;
- (void)unbound;

@end
