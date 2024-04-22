def get_weather(location):
    base_url = "https://api.openweathermap.org/data/2.5/weather?"

    complete_url = base_url + "q=" + location + "&appid=" + api_key + "&units=metric"
    
    response = requests.get(complete_url)
    weather_condition = response.json()["weather"][0]["description"]
    temperature = response.json()["main"]["temp"]
    
    return f"""Here is some information about the weather in {location}:\n
        - The weather is: {weather_condition}.\n
        - The temperature is: {temperature} degrees Celsius.\n
    """