# Image Messaging

Image messaging is a feature that enables your game to display a customisable popup message.  An image message is a type of [action](https://www.deltadna.net/deltadna/unitysdk/dev/engagements/actions) that is sent to a selection of users.  The Unity SDK has a
Popup class that draws the image message over your game.  The popup is event driven so you are
able to control when the image is downloaded from our servers, and when it is presented to the
player.

Each message consists of two parts, a sprite map contains images for the background and a number
of buttons, and a layout describing how to display these parts.  The layout uses constraints to
workout how to scale and where to place the popup.  This means we don't have to worry about
screen sizes, but we can also protect parts of the screen from being covered up.  The image is
drawn as large as possible within those constraints whilst still maintaining the original aspect
ratio.

## Engage Message Format

The image messages is an optional part of an Engage response. It is found under the `image` key.

### An Example


```javascript

{
    'url': 'http://cdn.com/image'
    'width': 1024,
    'height': 512,
    'format': 'png'
    'spritemap': {
        'background': {
            'x': 0,
            'y': 0,
            'width': 100,
            'height': 150
        },
        'buttons': [
            {
                'x': 102,
                'y': 0,
                'width': 25,
                'height': 10
            },
            {
                'x': 102,
                'y': 27,
                'width': 25,
                'height': 10
            }
        ]
    },
    'layout': {
        'landscape': {
            'background': {
                'cover': {
                    'halign': 'center'
                    'valign': 'center'
                },
                'action': {
                    'type': 'none'
                }
            },
            'buttons': [
                {
                    'x': 10,
                    'y': 25,
                    'action': {
                        'type' : 'action'
                        'value': 'BUY_GOLD'
                    }
                },
                {
                    'x': 50,
                    'y': 75,
                    'action': {
                        'type': 'dismiss'
                    }
                }
            ]
        },
    },
    'shim': {
        'mask': 'dimmed',   // none, clear, dimmed
        'action': {
            'type': 'dismiss'      // dismiss on touch
        }
    }
}

```

The `url` is the location of the sprite map image file.  The `width` and `height` are the size of
the image, and the `format` is the image format; PNG for now (this isn't used but is probably useful in the future).

The `spritemap` object describes the location of the image assets in the sprite map.  The `buttons`
are optional depending on the number of buttons in the message.

The `layout` object describes how the background is to appear on the screen.  It contains
`landscape` and/or `portrait` keys depending on preferred layout.  If only one key is present
the rules are applied whatever the orientation. (I think it would work this way, typically a game stays in the same orientation but this could be useful.)

The layout orientation contains rules for the background and the location of the buttons.  For the background two modes are valid: `cover` which scales the background image so it's as large as possible, and `contain` which makes the image as large as possible such that all the constraints are satisfied.  See below for more examples of `contain` layouts.  For the buttons the `x` and `y` coordinates are the top left corner of the buttons in the space of the background dimensions.

Each background and button object can have an action.  The action `type` can be *none*, *dismiss*, *link* or *action*.  If the type is *link* or *action* a `value` field will provide a string value to pass back to the callback associated with the button.  If a *link*, then the browser is opened automatically by the SDK.

The `shim` field describes how the remainder of the screen behind the message should be handled.  The `mask` can be either *none*, in which case nothing is added so any buttons behind the popup can still be clicked, *clear* which will have the effect of preventing background buttons from being clicked, and *dimmed* which greys out the screen.  The shim also supports actions so clicking on it can dismiss the popup too.

### Some More Layout Examples

Some more examples looking at the possible layouts and what they would achieve.

```javascript
'layout': {
    'landscape': {
        'background' : {
            'contain': {

            }
        }
    }
}
```
Fills the centre of the screen with the image.

```javascript
'layout': {
    'landscape': {
        'background' : {
            'contain': {
                'halign': 'left'
                'valign': 'center'
            }
        }
    }
}
```
Fills the screen with the image, positioning it against the left side.

```javascript
'layout': {
    'portrait': {
        'background': {
            'contain': {
                'halign': 'center',
                'valign': 'center',
                'left': '10px'
                'right': '20%'
                'top': '25px',
                'bottom': '5%'
            }
        }
    }
}
```
Fill's the screen with the image, but keeps at least 10 pixels from the left edge, 20% of the screen from the right, 25 pixels from the top and 5% of the screen from the bottom.  The image is then centered in the available space.

We can support different layouts for landscape and portrait by including both.

## Popup Class

To use a popup with your game, create a Popup object within a behaviour.  The popup is configured
by registering event handlers.  The ones you'll be most interested in are `AfterPrepare` and
`Action`.  The `AfterPrepare` event is triggered once the response from engage has happened.  The
popup can then be shown by calling `Show`.  The `Action` event is trigged once the player clicks on
a part of the popup you've registered an action for.  The event args report the name of the command
and the id of the part clicked allowing you to implement some custom behaviour.  

The SDK has a `RequestImageMessage` method which makes the call to Engage.  This is just a wrapper
around `RequestEngagement` that calls Prepare for you.  If you were using the other parts of the
engage response, such as any parameters you will want to make this call yourself.
