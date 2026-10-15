begin;
insert into applications (
	student_id,
	call_for_id,
	banned,
	deleted
) values
(26, 1, false, false),
(26, 2, false, false), 
(27, 1, false, false),
(28, 6, false, false),
(29, 1, false, false),
(30, 1, false, true),
(31, 1, false, false);
commit;
