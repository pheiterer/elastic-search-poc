import random
import io

# Configurações
NUM_RECORDS = 1_000_000
KEYWORDS = [
    "Coldplay", "Taylor", "Swift", "Lollapalooza", "Rock", "Rio", "Festival", "Show", 
    "Tour", "Live", "World", "Music", "Arena", "Stadium", "Concert", "Experience", 
    "Eras", "Spheres", "After", "Hours", "Til", "Dawn", "Future", "Past", "Magic",
    "Night", "Summer", "Winter", "Spring", "Autumn", "Global", "Electronic", "Jazz",
    "Pop", "Indie", "Alternative", "Metal", "Classical", "Hip", "Hop", "Dance",
    "Bruno", "Mars", "Metallica", "Beyoncé", "Drake", "Kendrick", "Lamar", "Weeknd",
    "U2", "Rolling", "Stones", "Beatles", "Experience", "Ultra", "Tomorrowland"
]

def generate_event_name():
    num_words = random.randint(2, 5)
    return " ".join(random.sample(KEYWORDS, num_words))

print(f"Gerando arquivo SQL com {NUM_RECORDS} registros...")

with open("seed_data.sql", "w") as f:
    f.write("BEGIN;\n")
    # Usando COPY para máxima performance no Postgres
    f.write("COPY events (name) FROM STDIN;\n")
    
    for i in range(NUM_RECORDS):
        name = generate_event_name()
        f.write(f"{name}\n")
        
        if (i + 1) % 100_000 == 0:
            print(f"Processados {i + 1} registros...")
            
    f.write("\\.\n")
    f.write("COMMIT;\n")

print("Arquivo seed_data.sql gerado com sucesso!")
print("Para importar, rode: docker cp seed_data.sql postgres:/seed_data.sql")
print("Depois: docker exec postgres psql -U user -d events_db -f /seed_data.sql")
