# M17UF3UnityServer
UnityServer
CREATE TABLE Races(
	id_race INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
    name VARCHAR(16),
    health INT,
    damage INT,
    valocity FLOAT,
    jumpforce FLOAT,
    cadence FLOAT
);
CREATE TABLE Users(
	id_user INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
    name VARCHAR(16),
    passwd VARCHAR(32),
    id_race_user INT,
    FOREIGN KEY (id_race_user) REFERENCES Races(id_race)
);
CREATE TABLE Versions(
	id_version INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
    last_version INT
);
