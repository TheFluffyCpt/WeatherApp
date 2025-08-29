# -*- coding: utf-8 -*-

import json
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.dates as mdates
import os
import sys

def create_graph(json_data):
    # ... (your existing data processing code) ...
    df = pd.json_normalize(json_data['list'])

    # Select and rename relevant columns
    df = df[['dt_txt', 'main.temp', 'wind.speed']]
    df.columns = ['Date', 'Temperature', 'Wind Speed']

    # Convert 'Date' column to datetime objects
    df['Date'] = pd.to_datetime(df['Date'])

    # Filter for 12:00 PM forecasts
    df_filtered = df[df['Date'].dt.hour == 12]

    # ... (your existing plotting code) ...
    fig, ax1 = plt.subplots(figsize=(10, 6))
    color = 'tab:red'
    ax1.set_xlabel('Date')
    ax1.set_ylabel('Temperature (C)', color=color)
    ax1.plot(df_filtered['Date'], df_filtered['Temperature'], color=color, marker='o', label='Temperature')
    ax1.tick_params(axis='y', labelcolor=color)
    ax1.grid(True, which='both', linestyle='--', linewidth=0.5)

    ax2 = ax1.twinx()
    color = 'tab:blue'
    ax2.set_ylabel('Wind Speed (m/s)', color=color)
    ax2.plot(df_filtered['Date'], df_filtered['Wind Speed'], color=color, marker='x', linestyle='--', label='Wind Speed')
    ax2.tick_params(axis='y', labelcolor=color)

    ax1.xaxis.set_major_locator(mdates.DayLocator(interval=1))
    ax1.xaxis.set_major_formatter(mdates.DateFormatter('%b-%d'))
    fig.autofmt_xdate(rotation=45)

    fig.suptitle('5-Day Forecast: Temperature and Wind Speed (12:00 PM)')
    fig.tight_layout()

    # --- THIS IS THE CRITICAL CHANGE ---
    # Construct the absolute path to the wwwroot folder
    # This assumes the script is in the project's root directory.
    script_dir = os.path.dirname(os.path.abspath(__file__))
    output_path = os.path.join(script_dir, 'wwwroot', 'forecast_graph.png')
    
    plt.savefig(output_path)
    
    return f"Graph generated and saved at {output_path}"

if __name__ == '__main__':
    try:
        # Check if the argument is a file path or a JSON string
        arg = sys.argv[1]
        
        # If the argument ends with .json, assume it's a file path
        if arg.endswith('.json'):
            with open(arg, 'r', encoding='utf-8') as f:
                json_data_str = f.read()
        else:
            json_data_str = arg
            
        json_data = json.loads(json_data_str)
        print(create_graph(json_data))
    except (json.JSONDecodeError, IndexError) as e:
        print(f"Error: Invalid JSON data or no data provided. {e}", file=sys.stderr)
        sys.exit(1)