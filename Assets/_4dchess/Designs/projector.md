# projector
like chess on a non-orthogonal board

## pieces
* pawns -> worker
	* 1 move per order
	* can only move on one direction, which counts as the worker's "next position". changing direction requires an action.
	* can only attack in tiles adjacent to both it's current position, and the next position
	* can move 2 in a straight line without going through other units if has not moved in at least 4 turns
	* after 6 moves (a double move counts as 2 moves), and reaching a 'dead end', promote to talent, leader, expert, or wizard
* knight -> talent
	* 2 moves per order, can move through other units, will not end turn immediately adjacent or in a straight line
* bishop -> leader
	* 3 moves per order, cannot move through other units, does not need a straight line
* rook -> expert
	* up to 6 moves per order, but all in a straight line (dot product identifies the new direction as being less than 30 degrees)
* queen -> wizard
	* up to 4 moves per order, or 6 in a straight line
* king -> director
	* 1 move per order
	* adjacent workers can move 2 per order
	* if another piece moves into his square he will move adjacent to where the other peice was

## gameplay
pieces try to claim goals by applying effort.
* 1 action per, turn usually a move command, a work command, or a goal discovery request
* effort takes 3 (base) turns to apply
* after applying 3 (base) turns of effort (experiences) to the same kind of goal, other similar goals take one less turn to apply effort
* all pieces can discover goals either on their tile or on an adjacent tile, which stay hidden unless a director asks for it
