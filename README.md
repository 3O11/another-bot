A discord bot made for automatic replies, written in C#.

## What can the bot do?

Currently, the bot does not have many features. The main feature is currently
the auto-reply functionality and (hopefully) very easy extensibility.

## What is planned for the future?

There are many things that chould be improved on the bot. Here are some of them
in no particular order.

- Improve the command parser
- Move all text into a separate JSON/XML file
- Improve Dialogue error handling

## Using the bot

Download and build the project (you will most likely want to build through
Visual Studio). After that, add a file called `botsettings.txt` to the
directory containing the `.exe` file and add a line that starts with `token=`
and specify your bot token after that. Then, you can launch the bot and use it
as you see fit.

There is one more option you can set in `botsettings.txt`, it's `botname`,
with which you can change the top-level bot keyword. More options will be added
in the future. It's also possible to write comments in the file. To mark a line
as a comment, add a `#` character at its start (whitespace is ignored).

After everything is set up and you have your instance of the bot up and
running, you might want to check out the `<botname> help` command to see where
to go from here.

## Programmer documentation

The focus here will be mainly a high-level overview of how the code is written
and structured, instead of a detailed explanation of each class. Some
implementation details are included as well though.

### Implementing bot extensions

Some thought has been given to making the bot as easily
extensible as possible. The easiest way of adding a new module is probably to
add a new class that inherits from `ModuleBase`. It has all the basic things
needed to work in the bot already implemented, so you can focus on adding your
own new commands and functionality. Don't forget that to actually enable your
module, you need to give it to the bot via `AddModule`.

To add a new command to your module, all you need to do is make a class that
implements `ICommand`, and add it to your module via the `AddCommand` method.
Please check to the implementation in `ReplyModule.cs` and `ReplyCommands.cs`
for a reference implementation.

Adding a custom dialogue is also very simple. You can either use the state
machine implementation that is provided in `DialogueBase.cs`, or you can
implement `IDialogue` directly and use your own approach. If you are making
your own dialogue implementation, please keep in mind that the dialogues have
precedence over commands, so you need to provide your own way of terminating
them. If you are implementing your dialogue by inheriting from the provided
`DialogueBase` implementation, please refere to the Dialogues section.

### Command structure

The basic idea behind commands is that a command should be a sequence of
keywords separated by a space. The keywords go from left to right in the order
of decreasing hierarchy.

```
<Bot name> <Module name> <Command name> <Command-specific options>
```

Of course, there are exceptions to this, like the help command. It made sense
to have it directly in the bot to have as few obstacles to getting help as
possible. The bot structure that gets created from this command structure is
exactly what is implemented, the bot holds modules (and commands), modules hold
commands, and they get picked based on the keywords specified.

### Dialogues

An implementation of a dialogue system based on state machines is provided. It
is entirely dynamic, as in, the programmer provides a transition function with
all the state transitions that they want (the trantition function can change
even at runtime!), and let the base implementation deal
with everything else. States are identified by strings of characters.

The state transitions are provided in the form of lambda functions, so to keep
persistent values between the transitions, they need to be specified directly
as fields in the class that holds the function.

The `DialogueBase` implementation recognizes three special states,
those are `start`, `final` and `error`. The `start` state is executed first
and going to the `final` state ends the dialogue. The `error` currently also
just terminates the dialogue, but there might be a bit more to it in the
future.

- All dialogues can be ended by the `terminate` keyword, regardless of the
user-provided transition function (as long as the implementation inherits
`DialogueBase`).
- The `IDialogue` interface is deliberately kept very simple to enable
different styles of dialogue implementation.

### Reply implementation

The reply system is currently the only more elaborate module.
The idea behind how it works is very simple. The user specifies what the bot
should reply to, what the reply should be, whom it should reply to, and where
it should reply. After that, the module automatically checks all messages and
replies to those that satisty all the constraints.

One of the things that are not ideal about the current reply implementation is
the way the reply data is stored. It has a list of replies for each server,
which is held inside a dictionary that takes the server ID as a key. This
data layout made the most sense when it comes to ease of implementation, since
The dictionary is thread safe and a list can be very easily "indexed" by all
manners of things (at the cost of performance).

### Performance

From the way the code is written, it will be very obvious that performance
was not the main focus in this codebase, the focus was on making the program
structure as well-written and extensible as possible. The reason for this is
that the code is mostly bound by network communication instead of inefficiencies
on the hot path.

### Other notes

- A command that arrives in the code is passed around in a custom
`MessageWrapper`, the reason for that was to simplify the process of parsing.

- Help for a command/module is specified directly in the class itself in the
appropriate pre-prepared field for it. The exact places are currently a bit
inconsistent, for commands, there is the `HelpText` getter, and for modules
there is the protected `_moduleDescription` field in the `ModuleBase` class.

- `Utils` contain some additional helper functions that did not fit anywhere
else.

- The current plan for better data storage in modules is to make a generic
"per-server" data structure that will also automatically take care of
serialization and deserialization. (In reality, it will only be a thin
wrapper on top of the existing `ConcurrentDictionary` implementation, with
some better abstraction of the internal `List`, ideally replaced with a more
suitable data structure.) The ideal solution would probably be to use a
database for the storage.
