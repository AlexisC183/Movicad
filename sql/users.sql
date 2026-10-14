BEGIN;

INSERT INTO users (key, role, password, creation, deleted) VALUES
('admin_01', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-01 08:00:00-07', false),
('admin_02', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-01 08:15:00-07', false),
('admin_03', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-01 08:30:00-07', false),
('admin_04', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-02 09:00:00-07', false),
('admin_05', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-02 09:20:00-07', false),
('admin_06', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-02 09:40:00-07', false),
('admin_07', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-03 10:00:00-07', false),
('admin_08', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-03 10:20:00-07', false),
('admin_09', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-03 10:40:00-07', false),
('admin_10', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-04 08:00:00-07', false),
('admin_11', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-04 08:25:00-07', false),
('admin_12', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-04 08:50:00-07', false),
('admin_13', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-05 09:10:00-07', false),
('admin_14', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-05 09:30:00-07', false),
('admin_15', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-05 09:50:00-07', false),
('admin_16', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-06 10:10:00-07', false),
('admin_17', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-06 10:30:00-07', false),
('admin_18', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-06 10:50:00-07', false),
('admin_19', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-07 08:10:00-07', false),
('admin_20', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-07 08:30:00-07', false),
('admin_21', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-07 08:50:00-07', false),
('admin_22', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-08 09:15:00-07', false),
('admin_23', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-08 09:35:00-07', false),
('admin_24', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-08 09:55:00-07', false),
('admin_25', 'administrativo', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-08-09 10:15:00-07', false);

INSERT INTO administratives
    (user_id, name, acronym, type, country_id, website, icon, icon_media_type)
SELECT
    u.user_id,
    a.name,
    a.acronym,
    a.type,
    a.country_id,
    a.website,
    ''::bytea,
    a.icon_media_type
FROM (
    VALUES
    ('admin_01', 'Universidad Nacional del Pacífico', 'UNP', 'publica', 138::bigint, 'https://www.unp.edu.example', 'image/png'),
    ('admin_02', 'Instituto Tecnológico del Norte', 'ITN', 'publica', 226::bigint, 'https://www.itn.edu.example', 'image/jpeg'),
    ('admin_03', 'Fundación Horizonte Educativo', 'FHE', 'privada', 52::bigint, 'https://www.horizonte.example', 'image/webp'),
    ('admin_04', 'Centro Comunitario La Esperanza', 'CCLE', 'comunitaria', 138::bigint, 'https://www.laesperanza.example', 'image/png'),
    ('admin_05', 'Academia Internacional del Valle', 'AIV', 'privada', 33::bigint, 'https://www.aiv.example', 'image/svg+xml'),
    ('admin_06', 'Universidad Metropolitana de Ciencias', 'UMC', 'publica', 142::bigint, 'https://www.umc.edu.example', 'image/png'),
    ('admin_07', 'Red Comunitaria Aprender', 'RCA', 'comunitaria', 1::bigint, 'https://www.aprender.example', 'image/gif'),
    ('admin_08', 'Instituto de Estudios Avanzados', 'IEA', 'privada', 81::bigint, 'https://www.iea.example', 'image/jpeg'),
    ('admin_09', 'Colegio Público del Centro', 'CPC', 'publica', 170::bigint, 'https://www.cpc.edu.example', 'image/png'),
    ('admin_10', 'Asociación Educativa Semilla', 'AES', 'comunitaria', 138::bigint, 'https://www.semilla.example', 'image/webp'),
    ('admin_11', 'Universidad del Sur Global', 'USG', 'privada', 14::bigint, 'https://www.usg.edu.example', 'image/svg+xml'),
    ('admin_12', 'Instituto Público de Innovación', 'IPI', 'publica', 112::bigint, 'https://www.ipi.edu.example', 'image/png'),
    ('admin_13', 'Centro Comunitario Nuevo Amanecer', 'CCNA', 'comunitaria', NULL, 'https://www.nuevoamanecer.example', 'image/jpeg'),
    ('admin_14', 'Escuela Superior de Negocios', 'ESN', 'privada', 201::bigint, 'https://www.esn.example', 'image/webp'),
    ('admin_15', 'Universidad Regional de Tecnología', 'URT', 'publica', 138::bigint, 'https://www.urt.edu.example', 'image/png'),
    ('admin_16', 'Fundación Puentes del Saber', 'FPS', 'comunitaria', 60::bigint, 'https://www.puentes.example', 'image/svg+xml'),
    ('admin_17', 'Instituto Privado de Idiomas', 'IPI2', 'privada', 75::bigint, 'https://www.idiomas.example', 'image/jpeg'),
    ('admin_18', 'Universidad Estatal del Lago', 'UEL', 'publica', 188::bigint, 'https://www.uel.edu.example', 'image/png'),
    ('admin_19', 'Centro Comunitario Raíces', 'CCR', 'comunitaria', 138::bigint, 'https://www.raices.example', 'image/gif'),
    ('admin_20', 'Academia Digital Andina', 'ADA', 'privada', 157::bigint, 'https://www.andina.example', 'image/webp'),
    ('admin_21', 'Instituto Nacional de Formación', 'INF', 'publica', 41::bigint, 'https://www.inf.edu.example', 'image/png'),
    ('admin_22', 'Fundación Comunidad Activa', 'FCA', 'comunitaria', NULL, 'https://www.comunidadactiva.example', 'image/jpeg'),
    ('admin_23', 'Universidad Privada San Marcos', 'UPSM', 'privada', 138::bigint, 'https://www.upsm.edu.example', 'image/svg+xml'),
    ('admin_24', 'Centro Público de Investigación', 'CPI', 'publica', 97::bigint, 'https://www.cpi.edu.example', 'image/png'),
    ('admin_25', 'Red de Educación Comunitaria', 'REC', 'comunitaria', 220::bigint, 'https://www.rec.example', 'image/webp')
) AS a(key, name, acronym, type, country_id, website, icon_media_type)
JOIN users AS u ON u.key = a.key;

COMMIT;

-- Human-generated
begin;
INSERT INTO users (key, role, password, creation, deleted) values
('estudiante1', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', false),
('estudiante2', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', false),
('estudiante3', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', false),
('estudiante4', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', false),
('estudiante5', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', false),
('estudiante6', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', true),
('estudiante7', 'estudiante', '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q', '2026-10-13', false);
insert into students values
(26, 'alex', ''::bytea, 'image/png'),
(27, 'Sergio', ''::bytea, 'image/png'),
(28, '', ''::bytea, 'image/png'),
(29, 'Martha', ''::bytea, 'image/png'),
(30, 'Alicia Sánchez', ''::bytea, 'image/png'),
(31, 'Letty', ''::bytea, 'image/png'),
(32, 'JP', ''::bytea, 'image/png');
commit;

-- For AI
/*create table users (
    user_id bigserial primary key,

    -- key is normalized to lowercase and must match the ECMAScript regex `^\w+$`
    key varchar(50) not null collate case_accent_insensitive
        unique
        check (length(key) > 0),
    role text not null collate case_accent_insensitive
        check (role in ('estudiante', 'administrativo')),

    -- insert '$2a$11$zxaBHeUWXW39XgAqCnhPAujqyEVPZeBGR47yMalGzA3nghPpJwe4q' as the password
    password char(60) not null,
    creation timestamp with time zone not null,
    deleted boolean not null
);

create table administratives (
    user_id bigint not null
        references users(user_id),
    name varchar(200) not null collate case_accent_insensitive,
    acronym varchar(100) not null collate case_accent_insensitive,
    type text not null collate case_accent_insensitive
        check (type in ('publica', 'privada', 'comunitaria')),

    -- valid country_ids: null or between 1 and 226
    country_id bigint
        references countries(country_id),
    website text not null collate case_accent_insensitive,

    -- insert an empty bytea as the icon
    icon bytea not null
        check (length(icon) <= 10485760),
    icon_media_type text not null collate case_accent_insensitive
        check (icon_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    primary key (user_id)
);*/
