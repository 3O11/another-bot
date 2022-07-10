# another-bot

A discord bot made for automatic replies, written in C#.

## What can the bot do?

Currently, the bot does not have many features. The main feature is currently
the auto-reply functionality and (hopefully) very easy extensibility.

## What is planned for the future?

There are many things that chould be improved on the bot. Here are some of them
in no particular order.

- Improve the command parser
- Move all text into a separate JSON/XML file
- Simplify IModule
- Make the replies persistent between restarts of the bot
- Move settings to a separate file

## Documentation

The User facing documentation is kept a bit short, because all commands
have help entries that explain their functionality directly on Discord.

### User documentation

To start using the bot, provide your bot token in `Program.cs` change the
keyword of the bot to what you want and start it. Some entry help is provided
in `<bot_keyword> help`, that also lists all commands available in all modules
and directly in the bot.

### Programmer documentation

The focus here will be mainly a high-level overview of how the code is written
and structured, instead of a detailed explanation of each class. Some
implementation details are included as well though.

#### Command structure

The basic idea behind commands is that a command should be a series of
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

#### Dialogues

An implementation of a dialogue system based on state machines is provided. It
is entirely dynamic, as in, the programmer provides a transition function with
all the state transitions that they want, and let the base implementation deal
with everything else. The states are identified by strings of characters. The
state transition are provided in the form of lambda functions, so as to keep
some kind of state between the transitions, all the persistent values need to
be directly in the class that holds the function.

- All dialogues can be ended by the `terminate` keyword, regardless of the
user-provided transition function (as long as the implementation inherits
`DialogueBase`).
- The `IDialogue` interface is deliberately kept very simple to enable
different styles of dialogue implementation.

#### Reply implementation

The reply system is currently the only more elaborate module implemented.
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

#### Performance

From the way the code is written, it will be painfully obvious that performance
was not the main focus in this codebase, the focus was on making the program
structure as well-written and extensible as possible. The reason for this is
that the code is mostly bound by network communication instead of inefficiencies
on the hot path.

#### Other notes

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
