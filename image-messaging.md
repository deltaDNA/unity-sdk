# Image Messaging

We've added a new feature into our platform called Image Messaging.  This enables your users to be
targeted with popup messages.  It's built on top of our Engagement system, so you can display
different popups, with different behaviour to different cohorts as you like.

Full support for image messaging is already available in our Unity SDK.  The SDK provides a default
implementation that displays the popop in front of your game.  Since every game has different
requirements we've made it easy to customise the behaviour to your needs.

## Getting Started

It's best to illustrate how image messaging works with an example.  I'll assume you already have
a Unity game set up on our platform.  To get started you will need to create a Targeting.  Go to
the __TODO__.  Select a decision point, when your game makes the call on our SDK, this is the
name you will need to use.  

Once the initial page is completed select the *messaging* tab (the others can be skipped since
we want to target all players).  Check the repeating box (so we can trigger the response multiple
times) and click the add message button.  The next panel let's you build the response that your
game will receive.  Since we're focusing on image messaging, select the image tab and then click
add image.

An image message consists of a background and up to two buttons.  On the left is a drop down for
selecting different viewports.  Viewports help you design your message and get a feel for how it
will appear on a real device.  On the right is a place for adding a background image.

### Image Sizes

Supporting different screen sizes can be challenging.  Our default approach is not to do any scaling
of the image.  So if you're targeting higher resolution screens you will need to upload larger
images to make them appear sensibly.  The viewports tool scales your image so you can see what the
uploaded image will look like for the player.  In the future we plan to support segmenting based on
screen size so different images can easily be composed for a range of device sizes.

### Buttons

By clicking the *Add Button* button, up to two buttons can be added to the message.  Select an image
for the button, it will be in the same pixel units as the background.

### Actions

Each image can be given an action, which will be triggered when the player taps or clicks the
image.  The *dismiss* action will close the popup.  The *deep link* action will launch the device
browser at the url.

### Enabling the Target

Once you're happy with the image, click the save message button.  This will take you to the summary
page.  You must then make the target live / unmute __TODO__.

## Using the SDK

At the point in your game where you want to get the popup, call `RequestImageMessage` on our SDK.
(If you look at the source, you'll see this method actually defers to `RequestEngagement` if
you're familiar with that.)  The `RequestImageMessage` method needs the *decision point*, which
must match the decision point chosen earlier for the Targeting, a set of *engage parameters*
and an optional callback.  The engage parameters __TODO__ can be an empty dictionary for this
example, once you start segmenting your targets you will add the keys in here.  The callback is
an additional function called when the popup loads, you could use it for pausing your game for
example.

There are two versions of `RequestImageMessage`, the version that doesn't take a template
parameter uses our default popup behaviour.  If you which to do something different write your
own popup behaviour and pass it in.

## Customising the Behaviour

If the default behaviour doesn't work for your game or you'd like to do something different,
override the Popup class and pass that to RequestImageMessage.  You will still be able to make
use of the helper classes explained below.

### Image Format

To write your own popup you need to understand the message format.  Engage returns a JSON object,
and inside this is the *image* key.  

    image
    |-- ur                          // sprite map location
    |-- width                       // width in pixels of the sprite map image
    |-- height                      // height in pixels of the sprite map image
    |-- viewport
    |   |-- width                   // width in pixels of the viewport
    |   |-- height                  // height in pixels of the viewport
    |-- background
    |   |-- x                       // left point of the sprite in the sprite map image
    |   |-- y                       // top point of the sprite in the sprite map image
    |   |-- imgX                    // left point inside viewport
    |   |-- imgY                    // top point inside viewport
    |   |-- width                   // sprite width in pixels
    |   |-- height                  // sprite height in pixels
    |   |-- actionType              // 'NONE', 'DISMISS' or 'LINK'
    |   |-- actionParam             // optional url for link
    |-- button1
    |   | etc...
    |-- button2
        | etc...

The coordinate convention is to have the x-axis at the left starting at zero, increasing to the
right.  The y-axis starts from zero at the top, increasing downwards.  All coordinates are
relative to the viewport.  This means the viewport should be aligned to screen for image to
display as intended.

The image JSON object describes how to compose the image.  The image is stored in a single sprite
map, this is located at the url value.  The *width* and *height* are the pixel dimensions of the
sprite map image. The *viewport* object describes the size of the viewport chosen for the image.
The remaining objects are known as *image assets* and each work the same way.  An image will have a
*background* and optionally *button1* and *button2*.

Each image asset contains *x* and *y* which are the location of the image in the sprite map image.
The *imgX* and *imgY* keys are the location relative to the viewport where the asset should be
drawn.  The *width* and *height* are the dimensions of the image.  The *actionType* can be either
NONE, DISMISS, or LINK and describes what should happen when the asset is selected.  If the
actionType is LINK an additional actionParam key is the url to trigger.

### Image Composition

Inside the Messaging namespace, the `ImageComposition` class provides the `BuildFromDictionary`
factory method.  This is used to create an ImageComposition object from the Engage image JSON.

### The Popup MonoBehaviour

The default behaviour is implemented by *Popup.cs* in the messaging namespace.  First it creates a
new `GameObject` for each image asset.  Then it downloads the sprite map with `WWW`.  Once the
sprite map has been downloaded it gets loaded into a `Texture2D`.  The image composition coordinates
are then translated onto the screen coordinates.  Unity's GUITexture feature is used to render the
sprites onto the screen.  This should put it in front of your game in most cases, but let us know if
it doesn't work for you.

A PopupHandlerBehaviour handles the user selecting the game objects.  This enables the actions to be
implemented.  Once the action is triggered, the Popup game object destroys itself, removing it  from
the scene.