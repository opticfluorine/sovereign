# Sovereign Engine To-Do Items

## Milestone 1

### Networking (Issue #8)

#### Automatically connect client to server on startup.
Client login user interface is out of scope for this issue, but it's interesting
so let's give it a shot.

##### Integrate Dear ImGui into client.
A new rendering stage is needed to render the GUI. This should be a two-stage
process following the existing rendering code. First a renderer-independent
stage is invoked, where the required ImGui calls are made and the buffers are
generated. Second the buffers are passed to the renderer, and the appropriate
graphics API calls are made to perform the GUI rendering.

##### Define client state machine.
The client needs to operate as a state machine that determines how the renderer
and input systems behave. Currently this needs a Main Menu state and an In Game
state. A mechanism is needed to facilitate the transition between states.

Eventually a third state, Character Selection, will be added between these two
states. This is beyond current scope.

##### Create login dialog in Main Menu state.
The client shall be able to display a modal login daialog box while in the
Main Menu state.

##### Attempt login through login dialog in Main Menu state.
The client shall attempt login when the user clicks the Login button on the
login dialog.

##### Display login errors to user in a dialog box.
The client shall display login errors in a modal dialog box until the user 
clicks OK.

##### Transition to In Game state upon successful login.
The client shall automatically transition to the In Game state upon a successful
login.

#### Add additional logging to registration and login events.
Not all cases are logged correctly right now. Fix this.

