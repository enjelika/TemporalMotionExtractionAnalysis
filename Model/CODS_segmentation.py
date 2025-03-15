# -*- coding: utf-8 -*-
"""
Created on Fri Mar 14 08:09:37 2025

@author: Debra Hogue

Description: RankNet CODS model used for segmentation of foreground object vs background
"""

import cv2
import os
import json
import numpy as np
from PIL import Image
import matplotlib.colors as color
import matplotlib.pyplot as plt
from skimage.measure import label, regionprops, find_contours
import tensorflow as tf
os.environ["CUDA_VISIBLE_DEVICES"] = '0'
from matplotlib.patches import Rectangle
import torch
import torch.nn.functional as F
from torch.autograd import Variable
import numpy as np
import pdb, os, argparse

from scipy import misc
from model.ResNet_models import Generator
from PIL import ImageFile
ImageFile.LOAD_TRUNCATED_IMAGES = True
import cv2

import sys
import time
import traceback

"""
===================================================================================================
    Helper function
        - Convert a mask to border image
===================================================================================================
"""
def mask_to_border(mask):
    # Convert PIL image (RGB), mask, to cv2 image (BGR), cv_mask
    open_cv_image = cv2.cvtColor(np.asarray(mask), cv2.COLOR_RGB2GRAY)
    
    # Get the height and width    
    h = open_cv_image.shape[0]
    w = open_cv_image.shape[1]
    border = np.zeros((h, w))

    contours = find_contours(open_cv_image, 1)
    for contour in contours:
        for c in contour:
            x = int(c[0])
            y = int(c[1])
            border[x][y] = 255

    return border

"""
===================================================================================================
    Helper function
        - Mask to bounding boxes
===================================================================================================
"""
def mask_to_bbox(mask):
    bboxes = []

    mask = mask_to_border(mask)
    lbl = label(mask)
    props = regionprops(lbl)
    
    for prop in props:
        x1 = prop.bbox[1]
        y1 = prop.bbox[0]

        x2 = prop.bbox[3]
        y2 = prop.bbox[2]

        bboxes.append([x1, y1, x2, y2])

    return bboxes

"""
===================================================================================================
    Helper function
        - Parsing mask for drawing bounding box(es) on an image
===================================================================================================
"""
def parse_mask(mask):
    mask = np.expand_dims(mask, axis=-1)
    mask = np.concatenate([mask, mask, mask], axis=-1)
    return mask


"""
===================================================================================================
    Helper function
        - Returns overlapping boxes 
        box format [xmin, xmax, ymin, ymax]
===================================================================================================
"""
def overlap(bbox1,bbox2):
    def overlap1D(b1,b2):
        return b1[1] >= b2[0] and b2[1] >= b1[0]
    
    return overlap1D(bbox1[:2],bbox2[:2]) and overlap1D(bbox1[2:],bbox2[2:])

"""
===================================================================================================
    Lvl 1 - Is anything present?
        Input:  binary_map & fix_map
        Output: fix_map (if a camouflaged object is detected)
===================================================================================================
"""
def levelOne(filename, binary_map, original_image, message, detect_fn):

    # Does the numpy array contain any non-zero values?
    all_zeros = not binary_map.any()
    
    if all_zeros:
        print("No object present.")
    else:
        print("Object detected.")
     
    return 


"""
===================================================================================================
    Helper Function - 
        Retrieves the overlap areas of weak camouflage within the binary map
===================================================================================================
"""
def apply_mask(heatmap, mask):
    # print("heatmap type:", type(heatmap))
    
    # Convert the heatmap to a NumPy array if it's an image
    if isinstance(heatmap, Image.Image):
        heatmap = np.asarray(heatmap)
        trans_heatmap = np.transpose(heatmap)
        # print("heatmap shape:", trans_heatmap.shape)
        
    # Broadcast the mask to match the shape of the heatmap
    mask_broadcasted = np.broadcast_to(mask, trans_heatmap.shape)
    
    # Apply the mask by multiplying the heatmap with the mask
    masked_heatmap = mask_broadcasted * trans_heatmap
    
    masked_heatmap = np.transpose(masked_heatmap)

    return masked_heatmap


"""
===================================================================================================
    IAI Function
===================================================================================================
"""
def segmentation(file_path):
    global RdBl, blGrRdBl, detect_fn
    
    try:
        log_step("findingSegmentation")
        
        #Turning off gpu since loading 2 models takes too much VRAM
        os.environ["CUDA_VISIBLE_DEVICES"] = "0"

        log_step("Initializing model")
        # Loading the model and handling the CUDA availability
        cods = Generator(channel=32)

        # Load the model with appropriate handling for CPU-only environments
        model_path = './models/Resnet/Model_50_gen.pth'
        if torch.cuda.is_available():
            cods.load_state_dict(torch.load(model_path))
            cods.cuda()
        else:
            cods.load_state_dict(torch.load(model_path, map_location=torch.device('cpu')))

        cods.eval()

        PATH_TO_SAVED_MODEL = "models/d7_f/saved_model"

        log_step("Loading TensorFlow model")
        detect_fn = []
        with tf.device('/CPU:0'):
            detect_fn = tf.saved_model.load(PATH_TO_SAVED_MODEL)
        log_step("TensorFlow model loaded successfully")
       
        log_step("Creating output directories")
        # Create output directories
        for dir in ["figures", "bbox_figures", "jsons", "outputs"]:
            if not os.path.exists(dir):
                os.mkdir(dir)
        
        file_name = os.path.splitext(os.path.basename(file_path))[0]
        if not os.path.exists('results'):
            os.makedirs('results')

        if not os.path.exists(f'outputs/{file_name}'):
            os.makedirs(f'outputs/{file_name}')
                        
        # XAI Message
        message = f"Decision for {file_name}: \n"
        
        log_step(f"Loading image: {file_name}")
        # Gather the images: Original, Binary Mapping, Fixation Mapping
        original_image = cv2.imread(file_path)
        if original_image is None:
            raise ValueError(f"Unable to load image from path: {file_path}")

        log_step("Preprocessing image")
        # Preprocess the image
        image = cv2.cvtColor(original_image, cv2.COLOR_BGR2RGB)
        image = cv2.resize(image, (224, 224))  # Adjust size as needed
        image = image.transpose((2, 0, 1))  # Change from HWC to CHW format
        image = image / 255.0  # Normalize to [0, 1]
        image = torch.from_numpy(image).float().unsqueeze(0)  # Add batch dimension

        # Get original image dimensions
        HH, WW = original_image.shape[:2]
        
        # Move image to GPU if available
        if torch.cuda.is_available():
            image = image.cuda()
        
        log_step("Running inference")
        # Get the Binary Mapping Prediction  
        _, _, cod_pred2 = cods.forward(image)
        
        log_step("Postprocessing results")
        # Process fixation and binary mapping predictions
        bm_image = process_prediction(cod_pred2, WW, HH)
        
        bm_image_pil = Image.fromarray(bm_image).convert('L')
        
        bm_image_pil.save(f'outputs/{file_name}/binary_image.png')

        # Normalize the Binary Mapping 
        trans_img = np.transpose(np.where(bm_image>0.5,1,0))
        img_np = np.asarray(trans_img)
                        
        log_step("Running through decision levels")
        levelOne(file_name, img_np, original_image, message, detect_fn)

        log_step("Saving output")
        org_image = Image.fromarray(cv2.cvtColor(original_image, cv2.COLOR_BGR2RGB))
        print(f'dim of org_image: {org_image.size}')
        print(f'dim of bm_image: {Image.fromarray(bm_image).size}')
    
        # Resize binary image to match original image size
        bm_image_resized = cv2.resize(bm_image, (org_image.width, org_image.height))
    
        bm_image_resized.save(f'results/segmented_{file_name}.jpg')

        log_step("segmentation complete")
        return (f'results/segmented_{file_name}.jpg')

    except Exception as e:
        error_message = f"An error occurred: {str(e)}\nTraceback: {traceback.format_exc()}"
        print(error_message)  # This will be captured by the redirected stdout in C#
        return f"Error occurred: {str(e)}"

def log_step(step_name):
    print(f"{time.time()}: Starting {step_name}")
    sys.stdout.flush()  # Ensure the output is immediately visible

def process_prediction(pred, WW, HH):
    pred = F.upsample(pred, size=[WW,HH], mode='bilinear', align_corners=False)
    pred = pred.sigmoid().data.cpu().numpy().squeeze()
    pred = 255*(pred - pred.min()) / (pred.max() - pred.min() + 1e-8)
    return pred.astype(np.uint8)
