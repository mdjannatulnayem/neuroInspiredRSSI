import pickle, json
import numpy as np
import pandas as pd
from flask import Flask, request, jsonify
from flask_cors import CORS


# Load the previously saved objects
try:
    with open('./model/simple_imputer.pkl', 'rb') as f:
        imputer = pickle.load(f)

    with open('./model/minmax_scaler.pkl', 'rb') as f:
        scaler = pickle.load(f)

    with open('./model/kmeans_model.pkl', 'rb') as f:
        clusterer = pickle.load(f)
    
    with open('./model/regression_model.pkl', 'rb') as f:
        regressor = pickle.load(f)
    
    with open('./config.json') as config_file:
        config = json.load(config_file)

except Exception as e:
    raise RuntimeError(f"Error loading model files: {e}")


# Define the clusterer mapping
mapping = {0: 0, 3: 1, 2: 2, 4: 3, 1: 4}

# Define columns to scale
column_names = [
    'month', 'day', 'hour', 'bx_gsm', 'by_gsm', 'bz_gsm', 'bt', 'speed', 'density', 'temp',
    'intensity', 'declination', 'inclination', 'north', 'east', 'vertical', 'horizontal'
]

app = Flask(__name__)

app.url_map.strict_slashes = False

CORS(app)

API_KEY = config.get('API_KEY')

# Middleware to check API key
@app.before_request
def check_api_key():
    if request.method != 'OPTIONS' and request.endpoint in ['classify','predict']:
        api_key = request.headers.get('Authorization')
        if api_key != f'Bearer {API_KEY}':
            return jsonify({'error': 'Unauthorized'}), 401


@app.route('/api/classify', methods=['POST'])
def classify():
    try:
        # Get the data from the POST request
        data = request.get_json()
        
        # Check if data is provided and is in the correct format
        if not data or not isinstance(data, dict):
            return jsonify({'error': 'Invalid input data format. Data should be a JSON object.'}), 400
        
        new_df = pd.DataFrame([data],columns=column_names)

        df_backup = new_df.copy()
        
        # Apply the imputer to handle missing values
        new_df[column_names] = imputer.transform(new_df)
        
        # Apply the scaler to scale the data
        new_df[column_names] = scaler.transform(new_df)
        
        # Predict the cluster labels
        y_predicted = clusterer.predict(new_df)

        # Add the cluster labels to the DataFrame
        df_backup['class'] = np.vectorize(mapping.get)(y_predicted)
        
        # Convert the DataFrame to JSON
        result = df_backup.to_dict(orient='records')
        
        return jsonify(result[0])
    
    except Exception as e:
        return jsonify({'error': f'An unexpected error occurred: {e}'}), 500



@app.route('/api/predict', methods=['POST'])
def predict():
    try:
        # Get the data from the POST request
        data = request.get_json()
        
        # Check if data is provided and is in the correct format
        if not data or not isinstance(data, dict):
            return jsonify({'error': 'Invalid input data format. Data should be a JSON object.'}), 400

        new_df = pd.DataFrame([data],columns=column_names)

        df_backup = new_df.copy()
        
        # Apply imputer and scaler transformations
        new_df[column_names] = imputer.transform(new_df)

        # Apply the scaler to scale the data
        new_df[column_names] = scaler.transform(new_df)
        
        # Drop unnecessary columns
        new_adjusted = new_df.drop(['intensity', 'declination', 'inclination', 'north', 'east', 'vertical', 'horizontal'], axis=1)
        
        # Predict bz_gsm using the loaded model
        new_df['bz_gsm'] = regressor.predict(new_adjusted)

        # Inverse transform to get back the original value
        new_original = pd.DataFrame(scaler.inverse_transform(new_df),columns=column_names)

        df_backup['bz_gsm_h'] = new_original['bz_gsm']
        
        # Convert the DataFrame to JSON
        result = df_backup.to_dict(orient='records')
        
        return jsonify(result[0])
    
    except Exception as e:
        return jsonify({'error': f'An unexpected error occurred: {e}'}), 500



if __name__ == '__main__':
    app.run(debug=True)
