# UML Diagrams for Knock Knock TCP Server

This directory contains PlantUML diagrams that visualize the architecture and conversation flow of the Knock Knock TCP Server.

## Files

- **ClassDiagram.puml** - UML Class Diagram showing the complete architecture
- **StateDiagram.puml** - UML State Diagram showing the conversation flow

## How to Use in Draw.io

### Option 1: Direct Import in Draw.io

1. Go to [draw.io](https://draw.io)
2. Click **File** → **Import from** → **URL**
3. Paste the URL to either `.puml` file (from your GitHub repository):
   - For ClassDiagram: `https://raw.githubusercontent.com/[USER]/[REPO]/master/Knock Knock TCP Server/UML/ClassDiagram.puml`
   - For StateDiagram: `https://raw.githubusercontent.com/[USER]/[REPO]/master/Knock Knock TCP Server/UML/StateDiagram.puml`
4. Draw.io will automatically render the PlantUML diagram

### Option 2: Using Online PlantUML Renderer

1. Go to [PlantUML Online Editor](http://plantuml.com/plantuml/uml/)
2. Copy the entire content of the `.puml` file
3. Paste it in the editor
4. The diagram renders automatically on the right side
5. Click **Export** to download as PNG, SVG, or PDF
6. You can then import the exported file into Draw.io

### Option 3: Converting PlantUML to Draw.io XML

1. Use the PlantUML Online Editor (Option 2)
2. Right-click the rendered diagram → **Save as**
3. Choose SVG format
4. Import the SVG into Draw.io: **File** → **Import from** → **Device**
5. Edit and customize as needed

### Option 4: Using Draw.io PlantUML Plugin

1. In Draw.io, go to **File** → **Preferences**
2. Enable **PlantUML Format** under export options
3. Create a new diagram, then go to **Arrange** → **Insert** → **Edit Diagram**
4. Paste the PlantUML content directly

## Diagram Overview

### Class Diagram (ClassDiagram.puml)

Shows the complete architecture of the Knock Knock TCP Server:

- **Strategy Pattern**: `IJokeSelector` interface with `RandomJokeSelector` and `SequentialJokeSelector` implementations
- **State Pattern**: `IKnockKnockState` interface with four state implementations:
  - `WaitingForJokeRequestState`
  - `WaitingForWhoState`
  - `SendingPunchlineState`
  - `LoopOrExitState`
- **Separation of Concerns**:
  - `Knock_KnockServer`: Handles network communication and threading
  - `ClientHandler`: Manages individual client conversations (implements `IConversationContext`)
  - States: Only know about `IConversationContext`, not about TCP or networking
- **Configuration Classes**: `JokeModel`, `JokesConfiguration`, `ServerOptions`

**Key Design Patterns Highlighted**:
- Strategy Pattern for flexible joke selection
- State Pattern for managing conversation flow
- Dependency Injection for loose coupling

### State Diagram (StateDiagram.puml)

Shows the complete conversation flow for one client:

**States**:
1. **WaitingForJokeRequest** (Initial)
   - Sends: "Knock knock?"
   - Expects: "joke" (case-insensitive)
   - Uses Strategy pattern to select a joke

2. **WaitingForWho**
   - Sends: "Knock! Knock!"
   - Expects: "Who's there?" (case-insensitive)

3. **SendingPunchline**
   - Sends the joke's "Who" part
   - Waits for any non-empty message
   - Sends the punchline

4. **LoopOrExit**
   - Asks: "Want another joke? (yes/no)"
   - "yes" → returns to WaitingForJokeRequest
   - "no" → ends conversation and disconnects

**Error Handling**:
- All states handle invalid input with error messages and loop back
- All states gracefully handle client disconnection

## Updating the Diagrams

As you develop and refine the architecture:

1. Update the `.puml` files in this directory
2. The changes will be reflected the next time you import or view them
3. Keep the diagrams in sync with the actual implementation

## Recommended Workflow

1. Keep PlantUML source files in version control
2. Use the PlantUML Online Editor for quick visualization
3. Import into Draw.io when you need to present or collaborate
4. Update both the diagrams and code together during development
5. Use the diagrams for design discussions and documentation

## PlantUML Syntax Reference

- [PlantUML Documentation](http://plantuml.com)
- [Class Diagram Syntax](http://plantuml.com/class-diagram)
- [State Diagram Syntax](http://plantuml.com/state-diagram)

## See Also

- `README.md` - Project overview
- `Program.cs` - Dependency Injection and configuration setup
- `ClientHandler.cs` - Main conversation orchestrator
- `States/` - All state implementations
- `Services/` - Strategy pattern implementations
