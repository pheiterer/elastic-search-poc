CREATE TABLE events (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

-- Enable logical replication for Debezium
ALTER TABLE events REPLICA IDENTITY FULL;

INSERT INTO events (name) VALUES ('Coldplay Music of the Spheres Tour');
INSERT INTO events (name) VALUES ('Taylor Swift Eras Tour');
INSERT INTO events (name) VALUES ('Lollapalooza 2024');
INSERT INTO events (name) VALUES ('Rock in Rio 2024');
INSERT INTO events (name) VALUES ('The Weeknd After Hours Til Dawn Tour');
INSERT INTO events (name) VALUES ('Bruno Mars Live in Brazil');
INSERT INTO events (name) VALUES ('Iron Maiden The Future Past Tour');
INSERT INTO events (name) VALUES ('Metallica M72 World Tour');
INSERT INTO events (name) VALUES ('Red Hot Chili Peppers Unlimited Love Tour');
INSERT INTO events (name) VALUES ('Beyoncé Renaissance World Tour');
