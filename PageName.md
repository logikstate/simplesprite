# Introduction #

This page contains a complete listing of the methods within the Sprite class.

# Public Methods #

## Play() ##
#### Overloads: ####
  * Play();
  * Play(String animationName);
Plays the currently queued animation, or the animation specified via overload.

Ex:
```
    Sprite sprite = this.GetComponent<Sprite>();
    sprite.Play("walk_animation");
```

## Stop() ##
Stops the current animation playing, and resets frame to 0.