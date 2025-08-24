--
-- PostgreSQL database dump
--

-- Dumped from database version 17.4
-- Dumped by pg_dump version 17.0

-- Started on 2025-08-24 16:10:18

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 221 (class 1259 OID 16495)
-- Name: incidents; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.incidents (
    id integer NOT NULL,
    premise_id integer NOT NULL,
    description character varying,
    created_by character varying NOT NULL,
    file_path character varying,
    date timestamp with time zone NOT NULL
);


ALTER TABLE public.incidents OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 16494)
-- Name: incidents_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.incidents ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.incidents_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 219 (class 1259 OID 16487)
-- Name: premises; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.premises (
    id integer NOT NULL,
    name character varying NOT NULL
);


ALTER TABLE public.premises OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 16486)
-- Name: premise_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.premises ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.premise_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 217 (class 1259 OID 16478)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    emp_no character varying NOT NULL,
    username character varying NOT NULL,
    is_admin boolean DEFAULT false NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 4312 (class 0 OID 16495)
-- Dependencies: 221
-- Data for Name: incidents; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (3, 1, 'Incident 1', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198b929-61d8-79fc-a07b-2211a401c4b7..jpeg', '2025-08-17 17:52:33.721+00');
INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (4, 1, 'Incident 2', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198b930-2b3b-732b-a370-96fb4dfe4709..jpg', '2025-08-17 17:59:58.466+00');
INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (5, 1, 'Incident 3', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198b931-14ac-71e4-82d0-52d8c23617bd..jpeg', '2025-08-17 18:00:58.267+00');
INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (6, 2, 'uuuuuuuu', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198b936-a9f5-7a47-9205-01d5db734c8c..jpeg', '2025-08-17 18:07:04.146+00');
INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (7, 1, 'meo con di lon ton', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198bd23-1a25-712e-a1c6-ebd08dc7b062..jpeg', '2025-08-18 12:24:10.987+00');
INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (8, 2, 'meo con di lon ton', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198c803-af7a-76c1-9a9d-9053010a3e71..jpeg', '2025-08-20 15:05:41.415+00');
INSERT INTO public.incidents OVERRIDING SYSTEM VALUE VALUES (9, 1, 'xe cua Dat di here', '88907299', 'https://scs-assessment-bucket.s3.ap-southeast-1.amazonaws.com/0198cb79-508c-7a19-ac26-9d08bd64b698..jpg', '2025-08-21 07:13:01.85+00');


--
-- TOC entry 4310 (class 0 OID 16487)
-- Dependencies: 219
-- Data for Name: premises; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.premises OVERRIDING SYSTEM VALUE VALUES (1, 'Premise 1');
INSERT INTO public.premises OVERRIDING SYSTEM VALUE VALUES (2, 'Premise 2');


--
-- TOC entry 4308 (class 0 OID 16478)
-- Dependencies: 217
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.users VALUES ('88907299', 'Huy', true);
INSERT INTO public.users VALUES ('88900001', 'Test', false);


--
-- TOC entry 4318 (class 0 OID 0)
-- Dependencies: 220
-- Name: incidents_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.incidents_id_seq', 9, true);


--
-- TOC entry 4319 (class 0 OID 0)
-- Dependencies: 218
-- Name: premise_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.premise_id_seq', 2, true);


--
-- TOC entry 4162 (class 2606 OID 16501)
-- Name: incidents incidents_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.incidents
    ADD CONSTRAINT incidents_pk PRIMARY KEY (id);


--
-- TOC entry 4160 (class 2606 OID 16493)
-- Name: premises premise_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.premises
    ADD CONSTRAINT premise_pk PRIMARY KEY (id);


--
-- TOC entry 4158 (class 2606 OID 16485)
-- Name: users users_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pk PRIMARY KEY (emp_no);


-- Completed on 2025-08-24 16:10:22

--
-- PostgreSQL database dump complete
--

