begin;
insert into applications (
	student_id,
	call_for_id,
	banned,
	deleted
) values
(26, 1, false, false), -- private chat, admin sends first
(26, 2, false, false), -- other, just applied
(27, 1, false, false), -- public chat
(28, 6, false, false), -- private chat other, student sends first
(29, 1, false, false), -- just applied
(30, 1, false, true),  -- left
(31, 1, false, false); -- private chat, student sends first, (deleted acc)
-- student 32 has no interactions
-- test call_for 3, which belongs to administrative 1
commit;
