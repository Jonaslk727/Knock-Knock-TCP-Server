# Knock-Knock TCP Server — Manual Test Cases

This document outlines 12 core manual test scenarios for validating the Knock-Knock TCP server functionality, concurrency, error handling, and robustness.

---

## Test Case 1: Single Client Connection and Conversation

**Objective:** Verify that a single client can successfully connect and complete a full Knock-Knock conversation.

**Preconditions:**
- Server is running on port 5000 with Random strategy
- Configuration: MaxClients = 10, Strategy = Random
- Sample jokes are loaded from appsettings.json

**Steps:**
1. Launch the Knock-Knock TCP Server application
2. Wait for server to display "Server started on port 5000"
3. Launch a TCP client (e.g., using Telnet or the test client utility)
4. Connect to localhost:5000
5. Wait for server response "Knock knock?"
6. Send: "Who's there?"
7. Receive: One of the joke setups (e.g., "Lettuce")
8. Send: "Lettuce who?"
9. Receive: The corresponding punchline (e.g., "Lettuce in, it's cold out here!")
10. Send: "Play again?"
11. Receive: Either the next joke or a goodbye message
12. Send: "No" or equivalent to exit
13. Disconnect gracefully

**Expected Results:**
- ✓ Client connects successfully
- ✓ All prompts and responses are received in correct order
- ✓ Conversation flows without errors
- ✓ Server logs show one active connection, then connection closed
- ✓ No exceptions or error messages in server console

**Acceptance Criteria:**
- [ ] Client receives all expected messages
- [ ] Messages contain correct joke content
- [ ] Conversation completes without timeouts
- [ ] Server remains responsive after disconnection

**Logging Points:**
- Server logs: "Client connected" and "Client disconnected"
- Message timestamps for each send/receive
- Thread information for the client handler

---

## Test Case 2: Three Simultaneous Clients

**Objective:** Verify that the server can handle multiple concurrent clients each running their own independent conversations.

**Preconditions:**
- Server is running on port 5000 with Random strategy
- MaxClients = 10

**Steps:**
1. Launch the Knock-Knock TCP Server
2. Open three separate TCP client connections to localhost:5000
3. For each client (let's call them A, B, C):
   - Simultaneously initiate conversations with slightly different timing (stagger by 1-2 seconds)
   - Client A completes full conversation (request → joke → answer → punchline → play again → accept)
   - Client B completes full conversation
   - Client C completes full conversation
4. Verify each client receives different or the same jokes (depends on strategy)
5. Disconnect all clients

**Expected Results:**
- ✓ All three clients connect simultaneously
- ✓ Each client receives independent joke sequences
- ✓ No message mixing between clients
- ✓ All clients complete conversations without interference
- ✓ Server logs show three concurrent active connections
- ✓ No race conditions or corrupted messages

**Acceptance Criteria:**
- [ ] All 3 clients successfully connect
- [ ] Each client completes full conversation independently
- [ ] Message integrity maintained (no partial/mixed messages)
- [ ] Server handles 3 clients without performance degradation
- [ ] Logs show correct thread IDs for each client

**Logging Points:**
- Thread IDs for each client handler
- Timestamp for each client connection/disconnection
- Memory usage during concurrent connections
- Lock/synchronization points if applicable

---

## Test Case 3: Client Sends Wrong Answer

**Objective:** Verify that the server correctly handles incorrect user responses and remains in the current state.

**Preconditions:**
- Server is running
- Configuration: Random strategy

**Steps:**
1. Connect a client to the server
2. Receive "Knock knock?" prompt
3. Send correct response: "Who's there?"
4. Receive a joke setup (e.g., "Boo")
5. **Send WRONG response:** "I don't know" (instead of "Boo who?")
6. Observe server behavior:
   - Server should either: stay in current state and re-prompt, or provide error message
7. Send correct response: "Boo who?"
8. Receive punchline
9. Continue conversation normally
10. Disconnect

**Expected Results:**
- ✓ Server remains stable (no crash)
- ✓ Server provides clear indication of wrong answer
- ✓ Server stays in waiting-for-answer state or re-prompts
- ✓ Conversation can recover and continue
- ✓ Logs show wrong answer attempt and recovery

**Acceptance Criteria:**
- [ ] Wrong answer does not crash server
- [ ] Server provides user-friendly error/guidance
- [ ] Conversation can continue after wrong answer
- [ ] No resource leaks
- [ ] Logs capture the error scenario

---

## Test Case 4: Client Disconnects Abruptly (Others Continue)

**Objective:** Verify that one client's abrupt disconnection doesn't affect other clients' conversations.

**Preconditions:**
- Server is running
- Configuration: MaxClients = 10

**Steps:**
1. Start two TCP client connections (A and B)
2. Both clients start conversations simultaneously
3. Client A receives joke setup and sends answer
4. **While A is responding, Client B abruptly disconnects** (close connection without sending "No")
5. Client A should finish or retry
6. Start a new Client C connection
7. Client C should connect and start conversation successfully
8. Disconnect A and C normally

**Expected Results:**
- ✓ Server does not crash or hang
- ✓ Client B's abrupt disconnect is handled gracefully
- ✓ Client A continues conversation unaffected
- ✓ New Client C can connect and start fresh
- ✓ Logs show clean cleanup of Client B (socket closed, thread ended)
- ✓ No orphaned connections or resource leaks

**Acceptance Criteria:**
- [ ] No server crash or hang
- [ ] Other clients unaffected
- [ ] Resources cleaned up for disconnected client
- [ ] New connections still accepted
- [ ] Thread count returns to normal after disconnect

**Logging Points:**
- Exact timestamp of abnormal disconnect
- Exception handling logs (if any)
- Thread cleanup confirmation
- Resource count before/after disconnect

---

## Test Case 5: Random Strategy Verification

**Objective:** Verify that Random joke strategy delivers jokes in random order (not sequential).

**Preconditions:**
- Server is running with Strategy = "Random"
- Jokes: ["Lettuce", "Tank", "Boo", "Interrupting cow", "Kanga", "App settings"]

**Steps:**
1. Connect multiple clients (5+) in sequence or concurrently
2. Each client completes one full conversation
3. Record the joke "Who" received by each client (e.g., first client gets "Boo", second gets "Lettuce", third gets "Tank")
4. Compare the sequence of jokes across clients
5. Optionally replay test multiple times to observe different sequences

**Expected Results:**
- ✓ Jokes are delivered in varying orders across multiple conversations
- ✓ Sequence is not deterministic (different order on different runs)
- ✓ All jokes in the collection are used over time
- ✓ No jokes are skipped or repeated unexpectedly

**Acceptance Criteria:**
- [ ] Jokes appear in different orders across test runs
- [ ] Sequence shows randomness (not predictable pattern)
- [ ] All jokes eventually used (over 10+ conversations)
- [ ] Random distribution is acceptable (not heavily biased)

**Logging Points:**
- Joke selection order for each conversation
- Random seed used (if logged)
- Selection timing and frequency per joke

---

## Test Case 6: Sequential Strategy Verification

**Objective:** Verify that Sequential strategy delivers jokes in consistent, repeating order.

**Preconditions:**
- Server is restarted with Strategy = "Sequential"
- Configuration file: appsettings.sequential.json (Jokes: ["TestSetup1", "TestSetup2"])

**Steps:**
1. Stop the current server
2. Start server with sequential strategy config
3. Connect Client 1, complete conversation, record joke received (should be "TestSetup1")
4. Disconnect
5. Connect Client 2, complete conversation, record joke received (should be "TestSetup2")
6. Disconnect
7. Connect Client 3, complete conversation, record joke received (should be "TestSetup1" again - cycle repeats)
8. Repeat 2-3 more times to confirm pattern

**Expected Results:**
- ✓ First client receives "TestSetup1"
- ✓ Second client receives "TestSetup2"
- ✓ Third client receives "TestSetup1" (cycle repeats)
- ✓ Sequence is deterministic and repeats exactly
- ✓ No randomness in joke selection

**Acceptance Criteria:**
- [ ] Jokes delivered in exact sequential order
- [ ] Sequence repeats correctly after last joke
- [ ] Pattern is consistent across multiple test runs
- [ ] No interruption to sequence

**Logging Points:**
- Current index in joke array
- Strategy selector logs showing sequential selection
- Cycle counter

---

## Test Case 7: New Joke Added to JSON (Server Restart)

**Objective:** Verify that new jokes can be added to the JSON configuration and are available after server restart.

**Preconditions:**
- Server is running
- Current jokes: ["Lettuce", "Tank", "Boo", "Interrupting cow", "Kanga", "App settings"]

**Steps:**
1. Stop the server
2. Edit appsettings.json
3. Add a new joke:
   ```json
   {
	 "Who": "NewJoke",
	 "Punchline": "This is a brand new joke!"
   }
   ```
4. Save the file
5. Restart the server
6. Connect multiple clients and complete conversations
7. Verify that "NewJoke" appears in the joke rotation

**Expected Results:**
- ✓ Server starts without errors
- ✓ New joke is loaded into memory
- ✓ New joke appears in future client conversations
- ✓ Old jokes continue to work
- ✓ No duplicate jokes or corrupted data

**Acceptance Criteria:**
- [ ] Server acknowledges new configuration on startup
- [ ] New joke appears in at least one client conversation
- [ ] Configuration validation passes
- [ ] No "joke not found" errors

**Logging Points:**
- Configuration reload logs
- Joke list size before/after
- Validation log entries

---

## Test Case 8: Maximum Clients Reached

**Objective:** Verify that the server correctly handles connection attempts when MaxClients limit is reached.

**Preconditions:**
- Server is restarted with MaxClients = 2
- Configuration: appsettings.maxclient1.json or custom

**Steps:**
1. Stop server and restart with MaxClients = 2
2. Connect Client 1 (should succeed)
3. Connect Client 2 (should succeed)
4. Attempt to connect Client 3 (MaxClients limit reached)
5. Observe server behavior:
   - Does it reject the connection?
   - Does it queue the connection?
   - Does it accept but disconnect some other client?
6. Depending on server design, verify appropriate handling
7. Disconnect Client 1
8. Attempt to connect Client 3 again (should now succeed)

**Expected Results:**
- ✓ First 2 clients connect successfully
- ✓ Third client connection is handled gracefully (not crash)
- ✓ Server provides feedback (rejection, queue message, or controlled disconnection)
- ✓ Third client can eventually connect after one client disconnects
- ✓ Logs clearly show max client limit detection

**Acceptance Criteria:**
- [ ] No server crash when limit reached
- [ ] Appropriate handling of excess connections
- [ ] Resources properly managed
- [ ] Error/status message is clear

**Logging Points:**
- Connection rejection/queueing timestamp
- Current active client count
- MaxClients configuration value

---

## Test Case 9: Invalid Configuration (Bad JSON)

**Objective:** Verify that the server provides clear error messages when configuration is invalid.

**Preconditions:**
- Server is stopped
- An invalid configuration exists: appsettings.invalid.json

**Steps:**
1. Stop the server
2. Temporarily replace appsettings.json with appsettings.invalid.json (or create a corrupt version)
3. Start the server
4. Observe error handling:
   - Does server start? (should not if validation is strict)
   - What error message is displayed?
   - Is the error message clear and actionable?
5. Check server logs for detailed error info

**Expected Results:**
- ✓ Server fails to start with clear error message
- ✓ Error message indicates the invalid field/value
- ✓ Error provides guidance (e.g., "Valid strategies: Random, Sequential")
- ✓ Server does not enter a partially-initialized state
- ✓ Logs contain full exception details for debugging

**Acceptance Criteria:**
- [ ] Startup fails gracefully
- [ ] Error message is user-friendly
- [ ] No corrupted state or orphaned resources
- [ ] Log provides developers with fix guidance

**Logging Points:**
- Full exception stack trace
- Validation failure details
- Which configuration property failed

---

## Test Case 10: Server Graceful Shutdown

**Objective:** Verify that the server shuts down cleanly, closing all connections and cleaning up resources.

**Preconditions:**
- Server is running with active clients
- 2-3 clients are in the middle of conversations

**Steps:**
1. Start server with 2-3 active client connections
2. Let clients be in various states (one mid-joke, one waiting for answer, etc.)
3. **Stop the server** (Ctrl+C or close the console window)
4. Observe server behavior:
   - How are active connections handled?
   - Are threads properly terminated?
   - Do clients receive disconnect notification?
5. Verify cleanup:
   - Check system resources (netstat, task manager)
   - Verify no orphaned TCP connections
   - Check for clean log exit messages

**Expected Results:**
- ✓ Server logs shutdown initiation message
- ✓ All active client connections are closed
- ✓ All threads are terminated
- ✓ No orphaned processes or sockets
- ✓ Clean exit message in logs (e.g., "Server has been stopped")
- ✓ Resources are fully released

**Acceptance Criteria:**
- [ ] Shutdown completes within reasonable time (< 5 seconds)
- [ ] No hanging connections or threads
- [ ] Logs show clean shutdown sequence
- [ ] No exceptions during shutdown

**Logging Points:**
- Shutdown initiation timestamp
- Each client thread termination
- Final resource cleanup message

---

## Test Case 11: Race Condition Stress Test

**Objective:** Verify that the server handles rapid concurrent connections and disconnections without race conditions.

**Preconditions:**
- Server is running
- Test client utility is available

**Steps:**
1. Use ConcurrentTestClientManager to stress test
2. Rapidly create/connect/disconnect 20-50 clients in quick succession
3. Monitor for:
   - Deadlocks (server becomes unresponsive)
   - Null reference exceptions
   - Socket exceptions
   - Memory leaks (connection count stability)
4. Run test for 2-5 minutes continuously
5. Verify shared state (joke list, client counter) remains consistent

**Expected Results:**
- ✓ No deadlocks (server remains responsive)
- ✓ No unhandled exceptions or crashes
- ✓ Connection/thread counts remain stable
- ✓ No memory leaks
- ✓ Server continues accepting new connections
- ✓ No corrupted shared data (jokes, configuration)

**Acceptance Criteria:**
- [ ] No exceptions or crashes during 5-minute stress test
- [ ] Server resource usage stable (no continuous growth)
- [ ] All rapid connections handled correctly
- [ ] Thread pool size reasonable

**Logging Points:**
- Active connection count over time
- Memory usage progression
- Thread count before/after stress test
- Any lock/synchronization warnings

---

## Test Case 12: Multiple Clients Wrong Answers vs. Others Continue

**Objective:** Verify that when one client sends wrong answers, other clients' conversations continue unaffected.

**Preconditions:**
- Server is running with 3 client slots available

**Steps:**
1. Connect 3 clients simultaneously
2. Client A: follows normal conversation flow
3. Client B: at some point sends WRONG answer (e.g., "I don't know" instead of correct "Who who?")
4. Client C: continues normal conversation unaffected
5. Observe:
   - Does Server handle Client B's wrong answer?
   - Does Server respond to correct messages from A and C?
   - Any interference between clients?
6. Verify all clients can eventually disconnect

**Expected Results:**
- ✓ Client B's wrong answer is handled independently
- ✓ Client A and C's conversations continue without interruption
- ✓ No message cross-contamination between clients
- ✓ Each client receives its own joke sequence
- ✓ All three clients complete or disconnect gracefully

**Acceptance Criteria:**
- [ ] Wrong answer does not affect other clients
- [ ] All three clients operate independently
- [ ] No race conditions or data corruption
- [ ] Logs show separate handling per client

**Logging Points:**
- Thread/client ID for each operation
- Timestamp for wrong answer
- Separate logs for each client's state machine transitions

---

## Test Summary Checklist

Use this checklist to track completion of all manual tests:

- [ ] Test 1: Single Client Connection (PASS/FAIL)
- [ ] Test 2: Three Simultaneous Clients (PASS/FAIL)
- [ ] Test 3: Wrong Answer Handling (PASS/FAIL)
- [ ] Test 4: Abrupt Disconnect (PASS/FAIL)
- [ ] Test 5: Random Strategy (PASS/FAIL)
- [ ] Test 6: Sequential Strategy (PASS/FAIL)
- [ ] Test 7: Dynamic Joke Update (PASS/FAIL)
- [ ] Test 8: Max Clients Limit (PASS/FAIL)
- [ ] Test 9: Invalid Configuration (PASS/FAIL)
- [ ] Test 10: Graceful Shutdown (PASS/FAIL)
- [ ] Test 11: Stress Test (Race Conditions) (PASS/FAIL)
- [ ] Test 12: Multiple Clients Mixed Behavior (PASS/FAIL)

**Overall Status:** [ ] ALL PASS [ ] SOME FAILURES [ ] CRITICAL ISSUES

---

## Notes for Testers

- Use server logs and client logs to correlate message flow
- Monitor system resource usage (CPU, Memory, Network) during concurrent tests
- Document any unexpected behavior with screenshots/logs
- Test on both Windows and Linux if possible (socket behavior differences)
- Verify thread safety by running stress tests multiple times
- Pay special attention to state transitions in state machine
