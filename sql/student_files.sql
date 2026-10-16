begin;
insert into student_files values
(5, 2, 'CONTRATO_ESCANEADO.pdf', 'application/pdf', '\x'::bytea, '2026-10-15T20:51:42.019', false),
(3, 7, 'Avance.pdf', 'application/pdf', '\x'::bytea, '2026-10-15T20:51:42.019', false),
(1, 4, 'compVisa.pdf', 'application/pdf', '\x'::bytea, '2026-10-15T20:51:42.019', false),
(1, 1, 'formato-solicitud.pdf', 'application/pdf', '\x'::bytea, '2026-10-15T20:51:42.019', false),
(5, 1, 'formato-solicitud.pdf', 'application/pdf', '\x'::bytea, '2026-10-15T20:51:42.019', true),
(1, 6, 'foto.jpg', 'image/jpeg', '\x'::bytea, '2026-10-15T20:51:42.019', false);
commit;
