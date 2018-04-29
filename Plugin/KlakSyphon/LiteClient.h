// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

// LiteClient: Simplified implementation of Syphon client

#import <Metal/MTLPixelFormat.h>

@protocol MTLDevice;
@protocol MTLTexture;

@interface LiteClient : NSObject

@property (readonly) id <MTLTexture> texture;
@property (readonly) BOOL isValid;

- (id)initWithServerDescription:(NSDictionary *)description;
- (void)updateWithDevice:(id <MTLDevice>)device pixelFormat:(MTLPixelFormat)format;

@end
