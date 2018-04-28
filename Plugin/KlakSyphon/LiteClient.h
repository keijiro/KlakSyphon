// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

// LiteClient: Simplified implementation of Syphon client

@protocol MTLDevice;
@protocol MTLTexture;

@interface LiteClient : NSObject

@property (readonly) id <MTLTexture> texture;
@property (readonly) BOOL isValid;

- (id)initWithServerDescription:(NSDictionary *)description;
- (void)updateWithDevice:(id <MTLDevice>)device;

@end
