begin;
insert into file_requests (
    key,
    title,
    call_for_id,
    deleted
) values
('B8029979B6A95EA61B406F123BA82B20', 'Formato de solicitud', 1, false),
('995241E951045CBF3CCD367B08E5F06A', 'Contrato', 1, true),
('EBAC0D67853F1359BD29329BFB69FBD3', 'Comprobante de estudios', 1, false),
('9596CD79ED1ABEFEC98D8D5070AECB6D', 'Comprobante de visa', 1, false),
('ECDF3EFF842096CE6E9412854AFD8DC9', 'e-firma', 1, false),
('F4A04227A6EFDB9B838338F741F49CE3', 'Foto blanco y negro, formal', 1, false),
('2E1B21937471672839AD40264676EDEC', 'Avance 1', 1, false);
commit;
