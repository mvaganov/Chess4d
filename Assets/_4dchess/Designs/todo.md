[x] identify if a move will put a king into check.
[x] block player from moving if it is not their turn
[x] get pieces at location from board state, not requiring a real board to exist.
[x] generate XFEN string from board state
[ ] calculate the next board state on hover
[/] do not allow a move that will put your own king into check
[ ] identify if a move will put your own king out of check
[ ] identify if there are not moves that can get the king out of check
-- ship it! --
[ ] reduce calls to expensive operations
[ ] cache calculations, and don't calculate if the results exist
[ ] evaluate board state for each player
[ ] evaluate the quality of each move