OUTPUT = KlakSyphon.dylib

CSRCS = Syphon/SyphonDispatch.c

MSRCS = \
LiteClient.m \
LiteServer.m \
PluginEntry.m \
Syphon/SyphonCFMessageReceiver.m \
Syphon/SyphonCFMessageSender.m \
Syphon/SyphonClientConnectionManager.m \
Syphon/SyphonMessageQueue.m \
Syphon/SyphonMessageReceiver.m \
Syphon/SyphonMessageSender.m \
Syphon/SyphonMessaging.m \
Syphon/SyphonPrivate.m \
Syphon/SyphonServerConnectionManager.m \
Syphon/SyphonServerDirectory.m

OBJS = $(CSRCS:.c=.o) $(MSRCS:.m=.o)

CFLAGS = -O2 -Wall -ISyphon -IUnity -Wno-deprecated-declarations
MFLAGS = $(CFLAGS) -include Syphon_Prefix.pch
LDFLAGS = -shared
LIBS = -framework Foundation -framework IOSurface -framework Metal -framework AppKit

ifdef TARGET
CFLAGS += -target $(TARGET)
MFLAGS += -target $(TARGET)
LDFLAGS += -target $(TARGET)
endif

all: $(OUTPUT)

clean:
	rm -f $(OUTPUT) $(OBJS)

$(OUTPUT): $(OBJS)
	gcc $(LDFLAGS) $(LIBS) -o $@ $^

.c.o:
	gcc $(CFLAGS) -c -o $@ $<

.m.o:
	gcc $(MFLAGS) -c -o $@ $<
