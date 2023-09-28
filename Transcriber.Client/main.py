import os
import re
import subprocess
import json
from typing import List, Dict, Any, Union
import argparse

import requests
import whisper

parser = argparse.ArgumentParser(description='Process options.')

parser.add_argument('--api-url',
                    type=str,
                    required=True,
                    help='The URL of the API, without trailing slash')

parser.add_argument('--file-directory',
                    type=str,
                    required=True,
                    help='The path where the video files are located')

base_api_url = parser.parse_args().api_url
input_file_directoy = parser.parse_args().file_directory


def parse_episode_info(filename: str) -> Union[dict[str, str], bool]:
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
    else:
        return False


def demux_audio(input_file):
    output_file = "output_audio.aac"
    subprocess.run(
        ["ffmpeg", "-i", input_file, "-vn", "-acodec", "copy", output_file, "-y", "-hide_banner", "-loglevel", "error"],
        stdout=subprocess.DEVNULL)
    return output_file


def transcribe_episode(audio_filename: str) -> list[dict[str, Any]]:
    model = whisper.load_model("medium")
    result = model.transcribe(audio_filename, verbose=False, language="German", fp16=False)

    simple_segments = []

    for value in result["segments"]:
        fiep = dict(
            start=value["start"],
            end=value["end"],
            text=value["text"].lstrip())

        simple_segments.append(fiep)

    return simple_segments


def send_episode_to_api(ingestible_episode_data):
    json_encoded = json.dumps(ingestible_episode_data, ensure_ascii=True)

    headers = {'Content-Type': 'application/json'}
    url = base_api_url + "/ingest"
    response = requests.post(url, headers=headers, data=json_encoded)
    print(response.status_code)


def check_episode_exists(episode_number: int) -> bool:
    url = base_api_url + "/exists"
    params = {'episode': episode_number}
    response = requests.get(url, params=params)
    return response.text.lower() == "true"


if __name__ == "__main__":
    print("Got api url: " + base_api_url)

    print("Looking for files …")

    for dirpath, dirnames, filenames in os.walk(input_file_directoy):
        for filename in filenames:
            absolute_file = os.path.join(dirpath, filename)
            print(f'Found file: {absolute_file}')

            parsed_episode_info = parse_episode_info(filename)

            if not parsed_episode_info:
                print("Found bogus file, skipping")
                continue

            if check_episode_exists(int(parsed_episode_info["number"])):
                print("Episode already done, skipping …")
                continue

            demuxed_audio_file = demux_audio(absolute_file)
            print("Demuxed audio")

            text_segments = transcribe_episode(demuxed_audio_file)
            ingestible_episode_data = dict(
                episode_number=parsed_episode_info["number"],
                episode_date=parsed_episode_info["date"],
                episode_title=parsed_episode_info["title"],
                segments=text_segments)

            print(f"Transcribed episode {ingestible_episode_data['episode_title']}")

            send_episode_to_api(ingestible_episode_data)
            print("Sent to API\r\n\r\n")
