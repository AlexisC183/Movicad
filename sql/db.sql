create collation case_accent_insensitive (
    provider = icu,
    locale = 'und-u-ks-level1',
    deterministic = false
);

create table users (
    user_id bigserial primary key,
    key varchar(50) not null collate case_accent_insensitive
        unique
        check (length(key) > 0),
    role text not null collate case_accent_insensitive
        check (role in ('estudiante', 'administrativo')),
    password char(60) not null,
    creation timestamp with time zone not null,
    deleted boolean not null
);

create table countries (
    country_id bigserial primary key,
    name text not null collate case_accent_insensitive
        unique
);

create table problems (
    problem_id bigserial primary key,
    author_email varchar(200) not null collate case_accent_insensitive,
    subject varchar(150) not null collate case_accent_insensitive,
    body text not null collate case_accent_insensitive
        check (length(body) > 0),
    creation timestamp with time zone not null,
    deleted boolean not null
);

create table students (
    user_id bigint not null
        references users(user_id),
    name varchar(100) not null collate case_accent_insensitive,
    icon bytea not null
        check (length(icon) <= 10485760),
    icon_media_type text not null collate case_accent_insensitive
        check (icon_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    primary key (user_id)
);

create table administratives (
    user_id bigint not null
        references users(user_id),
    name varchar(200) not null collate case_accent_insensitive,
    acronym varchar(100) not null collate case_accent_insensitive,
    type text not null collate case_accent_insensitive
        check (type in ('publica', 'privada', 'comunitaria')),
    country_id bigint
        references countries(country_id),
    website text not null collate case_accent_insensitive,
    icon bytea not null
        check (length(icon) <= 10485760),
    icon_media_type text not null collate case_accent_insensitive
        check (icon_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    primary key (user_id)
);

create table ratings (
    sender_id bigint not null
        references users(user_id),
    administrative_id bigint not null
        references administratives(user_id),
    primary key (sender_id, administrative_id)
);

create table administrative_messages (
    administrative_message_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    subject varchar(150) not null collate case_accent_insensitive,
    body text not null collate case_accent_insensitive
        check (length(body) > 0),
    creation timestamp with time zone not null,
    sender_id bigint not null
        references administratives(user_id),
    recipient_id bigint not null
        references administratives(user_id),
    deleted boolean not null
);

create table administrative_message_files (
    administrative_message_file_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    name text not null collate case_accent_insensitive,
    media_type text not null collate case_accent_insensitive,
    content bytea not null
        check (length(content) <= 31457280),
    administrative_message_id bigint not null
        references administrative_messages(administrative_message_id)
);

create table calls_for (
    call_for_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    title varchar(200) not null collate case_accent_insensitive
        check (length(title) > 0),
    initial_date timestamp with time zone,
    final_date timestamp with time zone,
    description text not null collate case_accent_insensitive,
    requirements text not null collate case_accent_insensitive,
    publish_date timestamp with time zone not null,
    modification timestamp with time zone not null,
    administrative_id bigint not null
        references administratives(user_id),
    deleted boolean not null
);

create table destination_countries (
    call_for_id bigint not null
        references calls_for(call_for_id),
    country_id bigint not null
        references countries(country_id),
    deleted boolean not null,
    primary key (call_for_id, country_id)
);

create table frequent_questions (
    frequent_question_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    question varchar(150) not null collate case_accent_insensitive
        check (length(question) > 0),
    answer text not null collate case_accent_insensitive
        check (length(question) > 0),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table links (
    link_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    title varchar(150) not null collate case_accent_insensitive
        check (length(title) > 0),
    url text not null collate case_accent_insensitive
        check (length(title) > 0),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table forum_files (
    forum_file_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    title varchar(150) not null collate case_accent_insensitive
        check (length(title) > 0),
    name text not null collate case_accent_insensitive,
    media_type text not null collate case_accent_insensitive,
    content bytea not null
        check (length(content) <= 31457280),
    modification timestamp with time zone not null,
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table forum_messages (
    forum_message_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    content varchar(1000) not null collate case_accent_insensitive
        check (length(content) > 0),
    creation timestamp with time zone not null,
    attached_image bytea not null
        check (length(attached_image) <= 10485760),
    image_media_type text not null collate case_accent_insensitive
        check (image_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    user_id bigint not null
        references users(user_id),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table file_requests (
    file_request_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    title varchar(150) not null collate case_accent_insensitive
        check (length(title) > 0),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table applications (
    application_id bigserial primary key,
    student_id bigint not null
        references students(user_id),
    call_for_id bigint not null
        references calls_for(call_for_id),
    banned boolean not null,
    deleted boolean not null,
    unique (student_id, call_for_id)
);

create table student_files (
    application_id bigint not null
        references applications(application_id),
    file_request_id bigint not null
        references file_requests(file_request_id),
    name text not null collate case_accent_insensitive,
    media_type text not null collate case_accent_insensitive,
    content bytea not null
        check (length(content) <= 31457280),
    modification timestamp with time zone not null,
    deleted boolean not null,
    primary key (application_id, file_request_id)
);

create table private_messages (
    private_message_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    content varchar(1000) not null collate case_accent_insensitive
        check (length(content) > 0),
    creation timestamp with time zone not null,
    attached_image bytea not null
        check (length(attached_image) <= 10485760),
    image_media_type text not null collate case_accent_insensitive
        check (image_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    user_id bigint not null
        references users(user_id),
    application_id bigint not null
        references applications(application_id),
    deleted boolean not null
);

create table expulsion_reasons (
    expulsion_reason_id bigserial primary key,
    key char(32) not null collate case_accent_insensitive
        unique,
    content varchar(300) not null collate case_accent_insensitive,
    creation timestamp with time zone not null,
    application_id bigint not null
        references applications(application_id),
    deleted boolean not null
);

INSERT INTO countries VALUES
(DEFAULT, 'afganistan'),
(DEFAULT, 'albania'),
(DEFAULT, 'alemania'),
(DEFAULT, 'andorra'),
(DEFAULT, 'angola'),
(DEFAULT, 'antigua y barbuda'),
(DEFAULT, 'arabia saudita'),
(DEFAULT, 'argelia'),
(DEFAULT, 'argentina'),
(DEFAULT, 'armenia'),
(DEFAULT, 'australia'),
(DEFAULT, 'austria'),
(DEFAULT, 'azerbaiyan'),
(DEFAULT, 'bahamas'),
(DEFAULT, 'banglades'),
(DEFAULT, 'barbados'),
(DEFAULT, 'barein'),
(DEFAULT, 'belgica'),
(DEFAULT, 'belice'),
(DEFAULT, 'benin'),
(DEFAULT, 'bielorrusia'),
(DEFAULT, 'birmania'),
(DEFAULT, 'bolivia'),
(DEFAULT, 'bosnia herzegovina'),
(DEFAULT, 'botsuana'),
(DEFAULT, 'brasil'),
(DEFAULT, 'brunei'),
(DEFAULT, 'bulgaria'),
(DEFAULT, 'burkina faso'),
(DEFAULT, 'burundi'),
(DEFAULT, 'butan'),
(DEFAULT, 'cabo verde'),
(DEFAULT, 'camboya'),
(DEFAULT, 'camerun'),
(DEFAULT, 'canada'),
(DEFAULT, 'catar'),
(DEFAULT, 'republica centroafricana'),
(DEFAULT, 'chad'),
(DEFAULT, 'republica checa'),
(DEFAULT, 'chile'),
(DEFAULT, 'china'),
(DEFAULT, 'chipre'),
(DEFAULT, 'colombia'),
(DEFAULT, 'comoras'),
(DEFAULT, 'congo'),
(DEFAULT, 'republica democratica del congo'),
(DEFAULT, 'corea del norte'),
(DEFAULT, 'corea del sur'),
(DEFAULT, 'costa de marfil'),
(DEFAULT, 'costa rica'),
(DEFAULT, 'croacia'),
(DEFAULT, 'cuba'),
(DEFAULT, 'dinamarca'),
(DEFAULT, 'dominica'),
(DEFAULT, 'republica dominicana'),
(DEFAULT, 'ecuador'),
(DEFAULT, 'egipto'),
(DEFAULT, 'el salvador'),
(DEFAULT, 'emiratos arabes unidos'),
(DEFAULT, 'eritrea'),
(DEFAULT, 'eslovaquia'),
(DEFAULT, 'eslovenia'),
(DEFAULT, 'espana'),
(DEFAULT, 'estados unidos'),
(DEFAULT, 'estonia'),
(DEFAULT, 'etiopia'),
(DEFAULT, 'filipinas'),
(DEFAULT, 'finlandia'),
(DEFAULT, 'fiyi'),
(DEFAULT, 'francia'),
(DEFAULT, 'gabon'),
(DEFAULT, 'gambia'),
(DEFAULT, 'georgia'),
(DEFAULT, 'ghana'),
(DEFAULT, 'granada'),
(DEFAULT, 'grecia'),
(DEFAULT, 'guatemala'),
(DEFAULT, 'guinea'),
(DEFAULT, 'guinea bisau'),
(DEFAULT, 'guinea ecuatorial'),
(DEFAULT, 'guyana'),
(DEFAULT, 'haiti'),
(DEFAULT, 'honduras'),
(DEFAULT, 'hungria'),
(DEFAULT, 'india'),
(DEFAULT, 'indonesia'),
(DEFAULT, 'irak'),
(DEFAULT, 'iran'),
(DEFAULT, 'irlanda'),
(DEFAULT, 'islandia'),
(DEFAULT, 'israel'),
(DEFAULT, 'italia'),
(DEFAULT, 'jamaica'),
(DEFAULT, 'japon'),
(DEFAULT, 'jordania'),
(DEFAULT, 'kazajistan'),
(DEFAULT, 'kenia'),
(DEFAULT, 'kirguistan'),
(DEFAULT, 'kiribati'),
(DEFAULT, 'kuwait'),
(DEFAULT, 'laos'),
(DEFAULT, 'lesoto'),
(DEFAULT, 'letonia'),
(DEFAULT, 'libano'),
(DEFAULT, 'liberia'),
(DEFAULT, 'libia'),
(DEFAULT, 'liechtenstein'),
(DEFAULT, 'lituania'),
(DEFAULT, 'luxemburgo'),
(DEFAULT, 'macedonia del norte'),
(DEFAULT, 'madagascar'),
(DEFAULT, 'malasia'),
(DEFAULT, 'malaui'),
(DEFAULT, 'maldivas'),
(DEFAULT, 'mali'),
(DEFAULT, 'malta'),
(DEFAULT, 'marruecos'),
(DEFAULT, 'islas marshall'),
(DEFAULT, 'mauricio'),
(DEFAULT, 'mauritania'),
(DEFAULT, 'mexico'),
(DEFAULT, 'micronesia'),
(DEFAULT, 'moldavia'),
(DEFAULT, 'monaco'),
(DEFAULT, 'mongolia'),
(DEFAULT, 'montenegro'),
(DEFAULT, 'mozambique'),
(DEFAULT, 'namibia'),
(DEFAULT, 'nauru'),
(DEFAULT, 'nepal'),
(DEFAULT, 'nicaragua'),
(DEFAULT, 'niger'),
(DEFAULT, 'nigeria'),
(DEFAULT, 'noruega'),
(DEFAULT, 'nueva zelanda'),
(DEFAULT, 'oman'),
(DEFAULT, 'paises bajos'),
(DEFAULT, 'pakistan'),
(DEFAULT, 'palaos'),
(DEFAULT, 'palestina'),
(DEFAULT, 'panama'),
(DEFAULT, 'papua nueva guinea'),
(DEFAULT, 'paraguay'),
(DEFAULT, 'peru'),
(DEFAULT, 'polonia'),
(DEFAULT, 'portugal'),
(DEFAULT, 'reino unido'),
(DEFAULT, 'ruanda'),
(DEFAULT, 'rumania'),
(DEFAULT, 'rusia'),
(DEFAULT, 'islas salomon'),
(DEFAULT, 'samoa'),
(DEFAULT, 'san cristobal y nieves'),
(DEFAULT, 'san marino'),
(DEFAULT, 'san vicente y las granadinas'),
(DEFAULT, 'santa lucia'),
(DEFAULT, 'santo tome y principe'),
(DEFAULT, 'senegal'),
(DEFAULT, 'serbia'),
(DEFAULT, 'seychelles'),
(DEFAULT, 'sierra leona'),
(DEFAULT, 'singapur'),
(DEFAULT, 'siria'),
(DEFAULT, 'somalia'),
(DEFAULT, 'sri lanka'),
(DEFAULT, 'esuatini'),
(DEFAULT, 'sudafrica'),
(DEFAULT, 'sudan'),
(DEFAULT, 'sudan del sur'),
(DEFAULT, 'suecia'),
(DEFAULT, 'suiza'),
(DEFAULT, 'surinam'),
(DEFAULT, 'tailandia'),
(DEFAULT, 'tanzania'),
(DEFAULT, 'tayikistan'),
(DEFAULT, 'timor oriental'),
(DEFAULT, 'togo'),
(DEFAULT, 'tonga'),
(DEFAULT, 'trinidad y tobago'),
(DEFAULT, 'tunez'),
(DEFAULT, 'turkmenistan'),
(DEFAULT, 'turquia'),
(DEFAULT, 'tuvalu'),
(DEFAULT, 'ucrania'),
(DEFAULT, 'uganda'),
(DEFAULT, 'uruguay'),
(DEFAULT, 'uzbekistan'),
(DEFAULT, 'vanuatu'),
(DEFAULT, 'ciudad del vaticano'),
(DEFAULT, 'venezuela'),
(DEFAULT, 'vietnam'),
(DEFAULT, 'yemen'),
(DEFAULT, 'yibuti'),
(DEFAULT, 'zambia'),
(DEFAULT, 'zimbabue'),
(DEFAULT, 'aruba'),
(DEFAULT, 'islas cook'),
(DEFAULT, 'curazao'),
(DEFAULT, 'groenlandia'),
(DEFAULT, 'islas feroe'),
(DEFAULT, 'islas marianas del norte'),
(DEFAULT, 'niue'),
(DEFAULT, 'puerto rico'),
(DEFAULT, 'san martin'),
(DEFAULT, 'sahara occidental'),
(DEFAULT, 'anguila'),
(DEFAULT, 'bermudas'),
(DEFAULT, 'islas caiman'),
(DEFAULT, 'gibraltar'),
(DEFAULT, 'guam'),
(DEFAULT, 'montserrat'),
(DEFAULT, 'nueva caledonia'),
(DEFAULT, 'islas pitcairn'),
(DEFAULT, 'polinesia francesa'),
(DEFAULT, 'samoa americana'),
(DEFAULT, 'santa elena ascension y tristan de acuna'),
(DEFAULT, 'tokelau'),
(DEFAULT, 'islas turcas y caicos'),
(DEFAULT, 'islas virgenes britanicas'),
(DEFAULT, 'islas virgenes de los estados unidos'),
(DEFAULT, 'abjasia'),
(DEFAULT, 'chipre del norte'),
(DEFAULT, 'kosovo'),
(DEFAULT, 'osetia del sur'),
(DEFAULT, 'somalilandia'),
(DEFAULT, 'taiwan');
