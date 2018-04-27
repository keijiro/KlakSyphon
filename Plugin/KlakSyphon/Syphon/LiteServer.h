@protocol MTLDevice;
@protocol MTLTexture;

@interface LiteServer : NSObject

@property (class) BOOL shouldConvertColorSpace;
@property (readonly) id <MTLTexture> texture;

- (id)initWithName:(NSString *)name dimensions:(NSSize)size device:(id <MTLDevice>)device;
- (void)publishNewFrame;

@end
