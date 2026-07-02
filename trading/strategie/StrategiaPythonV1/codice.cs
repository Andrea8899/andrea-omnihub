import speech_recognition as sr
from gtts import gTTS
import os
import io
import pygame
import requests
import json


# VARIABILI GLOBALI 
r = sr.Recognizer()
mioTesto = ""


def parlaChatAndrea(testo):
    try:
        tts = gTTS(text=testo, lang='it')
        audio_bytes = io.BytesIO()
        tts.write_to_fp(audio_bytes)
        audio_bytes.seek(0)

        pygame.mixer.init()
        pygame.mixer.music.load(audio_bytes)
        pygame.mixer.music.play()
        while pygame.mixer.music.get_busy():
            pygame.time.Clock().tick(10)
        pygame.mixer.quit()
    except pygame.error as e:
        print(f"Errore durante la riproduzione audio: {e}")
        return
    except Exception as e:
        print(f"Errore in parla: {e}")
        return
    

def gestisci_input(testo_input):
    testo_input_lower = testo_input.lower()
    #chiamata_lmStudio(testo_input)
    
    if "ciao" in testo_input_lower:
        parlaChatAndrea("Ciao anche a te!")
    elif "salva" in testo_input_lower:
        parlaChatAndrea("Ecco l'audio salvato.", salva_audio=True, nome_file="saluto.mp3")
    elif testo_input_lower in ("basta", "finisci", "stop", "fine"): # Controllo per l'uscita
        parlaChatAndrea("Arrivederci!")
        return True  # Indica che la conversazione deve terminare
    #else:
        #parlaChatAndrea("Non ho capito.")
    
    return False # Indica che la conversazione deve continuare



def richiestaLmStudio(messaggio) :
    url = "http://localhost:1234/v1/chat/completions"
    # Your request payload
    payload = {
        "messages": [
            { "role": "system","content":""},
            { "role": "user", "content": messaggio }
        ],
        "temperature": 0.7,
        "max_tokens": -1,
        "stream": False
    }
    headers = {
        "Content-Type": "application/json"
    }
    # Send POST request
    response = requests.post(url, headers=headers, data=json.dumps(payload))

    # Check for successful response
    if response.status_code == 200:
        return response.json()
    else:
        return {"error": "Request failed with status code " + str(response.status_code)}

def main():
    print("Welcome to the Chatbot! Type 'quit' to exit.")
    parlaChatAndrea("Ciao sono ChatAndrea")
    while True:
        with sr.Microphone() as source:
            print("Dimmi qualcosa (o dì 'basta', 'finisci', 'stop' o 'fine' per terminare):")
            try:
                audio = r.listen(source)
                mioTesto = r.recognize_google(audio, language="it-IT")
                print("Hai detto: " + mioTesto)

                if gestisci_input(mioTesto): # Chiama la funzione di gestione input
                    break # Esce dal loop se l'utente ha detto una parola di fine

            except sr.UnknownValueError:
                print("Non ho capito l'audio.")
            except sr.RequestError as e:
                print("Errore nel servizio di riconoscimento vocale; {0}".format(e))
            except KeyboardInterrupt: # Gestisce l'interruzione da tastiera (Ctrl+C)
                break
        
        risposta = richiestaLmStudio(mioTesto)
        rispostaBot = risposta.get("choices")[0].get("message").get("content") if risposta.get("choices") else "Sorry, I couldn't get a response."
        parlaChatAndrea(rispostaBot)
        print("Bot:", rispostaBot)

if __name__ == "__main__":
    main()
