-- Create database if not exists
CREATE DATABASE IF NOT EXISTS FoodApp;
USE FoodApp;

-- Drop existing tables if they exist
DROP TABLE IF EXISTS Recipe;
DROP TABLE IF EXISTS Admin;
DROP TABLE IF EXISTS Chef;
DROP TABLE IF EXISTS FoodLover;
DROP TABLE IF EXISTS Reviews;

CREATE TABLE FoodLover (
    foodlover_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    gender ENUM('Male', 'Female', 'Other') NOT NULL,
    contact_number VARCHAR(15) NOT NULL,
    address TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Chef (
    chef_id INT AUTO_INCREMENT PRIMARY KEY,
    image VARCHAR(255) NULL DEFAULT NULL,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    gender VARCHAR(10) NOT NULL,
    contact_number VARCHAR(15) NOT NULL,
    address TEXT NOT NULL,
    experience INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Admin (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Recipe (
    recipe_id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    ingredients TEXT NOT NULL,
    instructions TEXT NOT NULL,
    image VARCHAR(255) NOT NULL,
    rating DECIMAL(3, 2) DEFAULT 0.00,
    chef_id INT,
    foodlover_id INT,
    difficulty_level VARCHAR(50) NOT NULL DEFAULT 'easy',
    cuisine_type VARCHAR(50) NOT NULL,
    cooking_time INT NOT NULL,
    servings INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (chef_id) REFERENCES Chef(chef_id) ON DELETE SET NULL,
    FOREIGN KEY (foodlover_id) REFERENCES FoodLover(foodlover_id) ON DELETE SET NULL
);

-- Create Reviews table
CREATE TABLE IF NOT EXISTS Reviews (
    review_id INT AUTO_INCREMENT PRIMARY KEY,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    comment VARCHAR(500) NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    recipe_id INT NOT NULL,
    foodlover_id INT NOT NULL,
    FOREIGN KEY (recipe_id) REFERENCES Recipe(recipe_id) ON DELETE CASCADE,
    FOREIGN KEY (foodlover_id) REFERENCES FoodLover(foodlover_id) ON DELETE CASCADE
);

-- Add index on email for faster lookups
CREATE INDEX idx_chef_email ON Chef(email); 