@protocol MTLDevice;
@protocol MTLTexture;

@interface LiteClient : NSObject

@property (readonly) id <MTLTexture> texture;
@property (readonly) BOOL isValid;

- (id)initWithServerDescription:(NSDictionary *)description;
- (void)updateFromRenderThread:(id <MTLDevice>)device;

@end
