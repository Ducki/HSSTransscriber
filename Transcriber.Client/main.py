import os
import re
import subprocess
import json
import requests
import whisper


def parse_episode_info(filename: str) -> dict:
    match = re.search(r'- (\d+) - (\d{4}-\d{2}-\d{2}) - (.+)\.mp4', filename)

    if match:
        number = match.group(1)
        date = match.group(2)
        title = match.group(3)

        return dict(
            number=number,
            date=date,
            title=title
        )


def demux_audio(input_file):
    output_file = "output_audio.aac"
    subprocess.run(
        ["ffmpeg", "-i", input_file, "-vn", "-acodec", "copy", output_file, "-y", "-hide_banner", "-loglevel", "error"],
        stdout=subprocess.DEVNULL)
    return output_file


def transcribe_episode(audio_filename: str) -> dict:
    model = whisper.load_model("medium")
    result = model.transcribe(audio_filename, verbose=False, language="German")

    simple_segments = []

    for value in result["segments"]:
        fiep = dict(
            start=value["start"],
            end=value["end"],
            text=value["text"].lstrip())

        simple_segments.append(fiep)

    parsed_episode_info = parse_episode_info(filename)

    ingestable_stuff = dict(
        episode_number=parsed_episode_info["number"],
        episode_date=parsed_episode_info["date"],
        episode_title=parsed_episode_info["title"],
        segments=simple_segments)

    return ingestable_stuff


def send_episode_to_api(ingestible_episode_data):
    json_encoded = json.dumps(ingestible_episode_data, ensure_ascii=True)
    print(json_encoded)

    headers = {'Content-Type': 'application/json'}
    url = "http://localhost:5026/ingest"
    response = requests.post(url, headers=headers, data=json_encoded)
    print(response.status_code)


if __name__ == "__main__":
    # Loop through files
    directory = '/Users/alex/Downloads/hss'

    for dirpath, dirnames, filenames in os.walk(directory):
        for filename in filenames:
            absolute_file = os.path.join(dirpath, filename)
            print(f'Found file: {absolute_file}')

            demuxed_audio_file = demux_audio(absolute_file)
            print("Demuxed audio")

            ingestible_episode_data = transcribe_episode(demuxed_audio_file)
            print(f"Transcribed episode {ingestible_episode_data['episode_title']}")

            send_episode_to_api(ingestible_episode_data)
            print("Sent to API\r\n\r\n")