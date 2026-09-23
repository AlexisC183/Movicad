create table users (
    user_id bigserial primary key,
    key varchar(50) not null
        unique
        check (length(key) > 0),
    role text not null
        check (role in ('estudiante', 'administrativo')),
    password char(60) not null,
    creation timestamp with time zone not null,
    deleted boolean not null
);

create table countries (
    country_id bigserial primary key,
    name text not null
        unique
);

create table problems (
    problem_id bigserial primary key,
    author_email varchar(200) not null,
    subject varchar(150) not null,
    body text not null
        check (length(body) > 0),
    creation timestamp with time zone not null,
    deleted boolean not null
);

create table students (
    user_id bigint not null
        references users(user_id),
    name varchar(100) not null,
    icon bytea not null
        check (length(icon) <= 10485760),
    icon_media_type text not null
        check (icon_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    primary key (user_id)
);

create table administratives (
    user_id bigint not null
        references users(user_id),
    name varchar(200) not null,
    acronym varchar(100) not null,
    type text not null
        check (type in ('publica', 'privada', 'comunitaria')),
    country_id bigint
        references countries(country_id),
    website text not null,
    icon bytea not null
        check (length(icon) <= 10485760),
    icon_media_type text not null
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
    key char(32) not null
        unique,
    subject varchar(150) not null,
    body text not null
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
    key char(32) not null
        unique,
    name text not null,
    media_type text not null,
    content bytea not null
        check (length(content) <= 31457280),
    administrative_message_id bigint not null
        references administrative_messages(administrative_message_id)
);

create table calls_for (
    call_for_id bigserial primary key,
    key char(32) not null
        unique,
    title varchar(200) not null
        check (length(title) > 0),
    initial_date timestamp with time zone,
    final_date timestamp with time zone,
    description text not null,
    requirements text not null,
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
    key char(32) not null
        unique,
    question varchar(150) not null
        check (length(question) > 0),
    answer text not null
        check (length(question) > 0),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table links (
    link_id bigserial primary key,
    key char(32) not null
        unique,
    title varchar(150) not null
        check (length(title) > 0),
    url text not null
        check (length(title) > 0),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table forum_files (
    forum_file_id bigserial primary key,
    key char(32) not null
        unique,
    title varchar(150) not null
        check (length(title) > 0),
    name text not null,
    media_type text not null,
    content bytea not null
        check (length(content) <= 31457280),
    modification timestamp with time zone not null,
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table forum_messages (
    forum_message_id bigserial primary key,
    key char(32) not null
        unique,
    content varchar(1000) not null
        check (length(content) > 0),
    creation timestamp with time zone not null,
    attached_image bytea not null
        check (length(attached_image) <= 10485760),
    image_media_type text not null
        check (image_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    user_id bigint not null
        references users(user_id),
    call_for_id bigint not null
        references calls_for(call_for_id),
    deleted boolean not null
);

create table file_requests (
    file_request_id bigserial primary key,
    key char(32) not null
        unique,
    title varchar(150) not null
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
    name text not null,
    media_type text not null,
    content bytea not null
        check (length(content) <= 31457280),
    modification timestamp with time zone not null,
    deleted boolean not null,
    primary key (application_id, file_request_id)
);

create table private_messages (
    private_message_id bigserial primary key,
    key char(32) not null
        unique,
    content varchar(1000) not null
        check (length(content) > 0),
    creation timestamp with time zone not null,
    attached_image bytea not null
        check (length(attached_image) <= 10485760),
    image_media_type text not null
        check (image_media_type in ('image/gif', 'image/jpeg', 'image/png', 'image/webp', 'image/svg+xml')),
    user_id bigint not null
        references users(user_id),
    application_id bigint not null
        references applications(application_id),
    deleted boolean not null
);

create table expulsion_reasons (
    expulsion_reason_id bigserial primary key,
    key char(32) not null
        unique,
    content varchar(300) not null,
    creation timestamp with time zone not null,
    application_id bigint not null
        references applications(application_id),
    deleted boolean not null
);
