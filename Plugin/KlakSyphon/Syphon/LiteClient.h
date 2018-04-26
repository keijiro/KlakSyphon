@protocol MTLDevice;
@protocol MTLTexture;

@interface LiteClient : NSObject

@property (readonly) id <MTLTexture> texture;

- (id)initWithServerDescription:(NSDictionary *)description;
- (void)updateFromRenderThread:(id <MTLDevice>)device;

@end
