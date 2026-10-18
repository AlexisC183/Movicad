BEGIN;

INSERT INTO administrative_messages (
    key,
    subject,
    body,
    creation,
    sender_id,
    recipient_id,
    deleted
) VALUES
(
    '11aa22bb33cc44dd55ee66ff77889900',
    'Reunión de coordinación semestral',
    'Buenos días. Se convoca a reunión de coordinación para revisar las actividades del próximo semestre.',
    '2026-01-08 09:00:00-08',
    1,
    2,
    false
),
(
    '22bb33cc44dd55ee66ff77889900aa11',
    'Solicitud de actualización de datos',
    'Por favor, actualiza los datos de contacto de tu área antes del viernes.',
    '2026-01-10 11:15:00-08',
    2,
    5,
    false
),
(
    '33cc44dd55ee66ff77889900aa11bb22',
    'Confirmación de presupuesto',
    'Se confirma la disponibilidad presupuestal para las actividades de movilidad académica.',
    '2026-01-13 08:40:00-08',
    3,
    1,
    false
),
(
    '44dd55ee66ff77889900aa11bb22cc33',
    'Recordatorio de entrega de informes',
    'Les recordamos que el informe mensual debe entregarse a más tardar el día 25.',
    '2026-01-15 14:20:00-08',
    4,
    7,
    false
),
(
    '55ee66ff77889900aa11bb22cc33dd44',
    'Invitación a capacitación',
    'Se adjunta la invitación a la capacitación sobre procesos de internacionalización.',
    '2026-01-18 10:00:00-08',
    5,
    9,
    false
),
(
    '66ff77889900aa11bb22cc33dd44ee55',
    'Validación de convocatoria',
    'La convocatoria fue revisada y puede continuar con el proceso de publicación.',
    '2026-01-20 16:30:00-08',
    6,
    3,
    false
),
(
    '77889900aa11bb22cc33dd44ee55ff66',
    'Cambio de fecha para la sesión',
    'La sesión programada para mañana se moverá al jueves a las 10:00 horas.',
    '2026-01-22 13:10:00-08',
    7,
    12,
    false
),
(
    '889900aa11bb22cc33dd44ee55ff6677',
    'Consulta sobre convenio internacional',
    '¿Podrían compartir el estado actual del convenio con la institución asociada?',
    '2026-01-25 09:35:00-08',
    8,
    4,
    false
),
(
    '9900aa11bb22cc33dd44ee55ff667788',
    'Aprobación de propuesta de movilidad',
    'La propuesta presentada ha sido aprobada. Favor de iniciar los trámites correspondientes.',
    '2026-01-28 15:45:00-08',
    9,
    15,
    false
),
(
    '00aa11bb22cc33dd44ee55ff66778899',
    'Documentación pendiente',
    'Falta recibir la carta de aceptación y la constancia de seguro médico.',
    '2026-02-01 08:25:00-08',
    10,
    6,
    false
),
(
    'aabbccddeeff00112233445566778899',
    'Seguimiento a solicitud de beca',
    'Se informa que la solicitud de beca se encuentra en etapa de revisión documental.',
    '2026-02-03 12:00:00-08',
    11,
    18,
    false
),
(
    'bbccddeeff00112233445566778899aa',
    'Actualización de calendario académico',
    'Se comparte el calendario académico actualizado para el periodo agosto-diciembre.',
    '2026-02-06 10:50:00-08',
    12,
    8,
    false
),
(
    'ccddeeff00112233445566778899aabb',
    'Solicitud de difusión',
    'Solicitamos apoyo para difundir la convocatoria entre estudiantes y docentes.',
    '2026-02-10 09:10:00-08',
    13,
    20,
    false
),
(
    'ddeeff00112233445566778899aabbcc',
    'Confirmación de participación',
    'Confirmamos la participación de nuestra institución en el encuentro internacional.',
    '2026-02-12 17:05:00-08',
    14,
    11,
    false
),
(
    'eeff00112233445566778899aabbccdd',
    'Revisión de expediente',
    'El expediente fue recibido y será revisado por el comité durante esta semana.',
    '2026-02-15 11:30:00-08',
    15,
    22,
    false
),
(
    'ff00112233445566778899aabbccddee',
    'Solicitud de reporte de actividades',
    'Favor de enviar el reporte de actividades realizadas durante el primer trimestre.',
    '2026-02-18 08:55:00-08',
    16,
    13,
    false
),
(
    '00112233445566778899aabbccddeeff',
    'Aviso de mantenimiento del sistema',
    'El sistema estará en mantenimiento el sábado de 08:00 a 12:00 horas.',
    '2026-02-20 14:40:00-08',
    17,
    21,
    false
),
(
    '1234567890abcdef1234567890abcdef',
    'Resultado de evaluación de proyecto',
    'El proyecto fue evaluado favorablemente y se autoriza la siguiente etapa.',
    '2026-02-23 16:15:00-08',
    18,
    16,
    false
),
(
    'fedcba0987654321fedcba0987654321',
    'Coordinación de visita institucional',
    'Se solicita confirmar la agenda propuesta para la visita institucional de marzo.',
    '2026-02-25 10:20:00-08',
    19,
    24,
    false
),
(
    'abcdef0123456789abcdef0123456789',
    'Cierre de convocatoria',
    'La convocatoria cerró correctamente y se iniciará la evaluación de postulaciones.',
    '2026-02-28 18:00:00-08',
    20,
    17,
    false
);

INSERT INTO administrative_message_files (
    key,
    name,
    media_type,
    content,
    administrative_message_id
)
SELECT
    f.key,
    f.name,
    f.media_type,
    f.content,
    m.administrative_message_id
FROM (
    VALUES
    (
        '101112131415161718191a1b1c1d1e1f',
        'agenda_reunion_semestral.pdf',
        'application/pdf',
        '\x255044462d312e34'::bytea,
        '11aa22bb33cc44dd55ee66ff77889900'
    ),
    (
        '202122232425262728292a2b2c2d2e2f',
        'lista_asistentes.xlsx',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        ''::bytea,
        '11aa22bb33cc44dd55ee66ff77889900'
    ),
    (
        '303132333435363738393a3b3c3d3e3f',
        'formato_actualizacion_datos.docx',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        ''::bytea,
        '22bb33cc44dd55ee66ff77889900aa11'
    ),
    (
        '404142434445464748494a4b4c4d4e4f',
        'presupuesto_movilidad_2026.pdf',
        'application/pdf',
        '\x255044462d312e37'::bytea,
        '33cc44dd55ee66ff77889900aa11bb22'
    ),
    (
        '505152535455565758595a5b5c5d5e5f',
        'plantilla_informe_mensual.xlsx',
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        ''::bytea,
        '44dd55ee66ff77889900aa11bb22cc33'
    ),
    (
        '606162636465666768696a6b6c6d6e6f',
        'invitacion_capacitacion.pdf',
        'application/pdf',
        '\x255044462d312e35'::bytea,
        '55ee66ff77889900aa11bb22cc33dd44'
    ),
    (
        '707172737475767778797a7b7c7d7e7f',
        'programa_capacitacion.png',
        'image/png',
        '\x89504e470d0a1a0a'::bytea,
        '55ee66ff77889900aa11bb22cc33dd44'
    ),
    (
        '808182838485868788898a8b8c8d8e8f',
        'convocatoria_validada.pdf',
        'application/pdf',
        ''::bytea,
        '66ff77889900aa11bb22cc33dd44ee55'
    ),
    (
        '909192939495969798999a9b9c9d9e9f',
        'calendario_sesion.ics',
        'text/calendar',
        ''::bytea,
        '77889900aa11bb22cc33dd44ee55ff66'
    ),
    (
        'a0a1a2a3a4a5a6a7a8a9aaabacadaeaf',
        'estado_convenio.docx',
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        ''::bytea,
        '889900aa11bb22cc33dd44ee55ff6677'
    ),
    (
        'b0b1b2b3b4b5b6b7b8b9babbbcbdbebf',
        'propuesta_movilidad.pdf',
        'application/pdf',
        '\x255044462d312e36'::bytea,
        '9900aa11bb22cc33dd44ee55ff667788'
    ),
    (
        'c0c1c2c3c4c5c6c7c8c9cacbcccdcecf',
        'lista_documentos_pendientes.pdf',
        'application/pdf',
        ''::bytea,
        '00aa11bb22cc33dd44ee55ff66778899'
    ),
    (
        'd0d1d2d3d4d5d6d7d8d9dadbdcdddedf',
        'calendario_academico_2026.pdf',
        'application/pdf',
        '\x255044462d312e34'::bytea,
        'bbccddeeff00112233445566778899aa'
    ),
    (
        'e0e1e2e3e4e5e6e7e8e9eaebecedeeef',
        'material_difusion.zip',
        'application/zip',
        ''::bytea,
        'ccddeeff00112233445566778899aabb'
    ),
    (
        'f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff',
        'agenda_visita_institucional.pdf',
        'application/pdf',
        ''::bytea,
        'fedcba0987654321fedcba0987654321'
    )
) AS f(key, name, media_type, content, message_key)
JOIN administrative_messages AS m ON m.key = f.message_key;

COMMIT;

-- For AI
/*
create table administrative_messages (
    administrative_message_id bigserial primary key,

    -- Random hex number string of length 32
    key char(32) not null collate case_accent_insensitive
        unique,

    subject varchar(150) not null collate case_accent_insensitive,
    body text not null collate case_accent_insensitive
        check (length(body) > 0),
    creation timestamp with time zone not null,

    -- Valid IDs between 1 and 25
    sender_id bigint not null
        references administratives(user_id),
    recipient_id bigint not null
        references administratives(user_id),

    deleted boolean not null
);

create table administrative_message_files (
    administrative_message_file_id bigserial primary key,

    -- Random hex number string of length 32
    key char(32) not null collate case_accent_insensitive
        unique,

    -- File name + extension
    name text not null collate case_accent_insensitive,

    media_type text not null collate case_accent_insensitive,

    -- Random bytea literal or ''::bytea
    content bytea not null
        check (length(content) <= 31457280),

    administrative_message_id bigint not null
        references administrative_messages(administrative_message_id)
);*/
