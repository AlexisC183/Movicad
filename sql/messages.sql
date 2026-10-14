begin;
insert into private_messages values
(default, 'A5865FB2B6350FC97E1A770056DF5230', 'Aquí tienes el QR para solicitar', '2026-10-10T15:43:22.019', ''::bytea, 'image/png', 1, 1, false),
(default, '9567816BBA592B7BF8F398B9BA6088B1', 'muchas gracias', '2026-10-10T16:00:01.099', ''::bytea, 'image/gif', 26, 1, false),
(default, 'CEFC82E514B31AD7DDCF3A9A40C70B91', 'disculpe, existe un limite de cupo?', '2026-10-11T07:22:01.610', ''::bytea, 'image/gif', 28, 4, false),
(default, '24F9517B18AB24BD7C9300A1C6518E48', 'Hola', '2026-10-04T22:30:00.001', ''::bytea, 'image/gif', 31, 7, false);
commit;

begin;
insert into forum_messages values
(default, 'B341F47E236944D0D06D539F9ABF294E', 'Soy nuevo!!', '2026-10-13T06:00:30.812', ''::bytea, 'image/gif', 27, 1, false);
commit;
