[x] identify if a move will put a king into check.
[x] block player from moving if it is not their turn
[x] get pieces at location from board state, not requiring a real board to exist.
[x] generate XFEN string from board state
[x] create MoveCalculator service that will manage moves and their calculations
[x] calculate the next board state on move hover
[x] do not allow a move that will put your own king into check
[x] identify if a move will put your own king out of check
[ ] have the Board reflect the GameState instead of the GameState reflecting the board
[ ] identify if there are not moves that can get the king out of check, and show end of game state
[ ] restart game controls
-- ship it! --
[ ] save game state as string
[ ] load game state from string
[ ] reduce calls to expensive operations
[ ] MoveCalculator does it's work asynchronously
[ ] cache calculations, and don't calculate if the results exist
[ ] evaluate board state for each player
[ ] evaluate the quality of each move
