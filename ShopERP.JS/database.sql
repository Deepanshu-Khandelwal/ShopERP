-- =====================================
-- CREATE DATABASE
-- =====================================
CREATE DATABASE IF NOT EXISTS medical_store;
USE medical_store;

-- =====================================
-- USERS
-- =====================================
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE,
    password VARCHAR(255),
    role ENUM('admin', 'staff') DEFAULT 'staff',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================
-- MEDICINES
-- =====================================
CREATE TABLE medicines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    company VARCHAR(100),
    salt VARCHAR(150),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================
-- BATCHES (STOCK CORE)
-- =====================================
CREATE TABLE batches (
    id INT AUTO_INCREMENT PRIMARY KEY,
    medicine_id INT,
    batch_no VARCHAR(50),
    expiry_date DATE,
    purchase_price DECIMAL(10,2),
    mrp DECIMAL(10,2),
    quantity INT DEFAULT 0,
    FOREIGN KEY (medicine_id) REFERENCES medicines(id)
);

-- =====================================
-- PURCHASE
-- =====================================
CREATE TABLE purchases (
    id INT AUTO_INCREMENT PRIMARY KEY,
    supplier_name VARCHAR(150),
    bill_no VARCHAR(50),
    total_amount DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================
-- PURCHASE ITEMS
-- =====================================
CREATE TABLE purchase_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    purchase_id INT,
    batch_id INT,
    quantity INT,
    purchase_price DECIMAL(10,2),
    FOREIGN KEY (purchase_id) REFERENCES purchases(id),
    FOREIGN KEY (batch_id) REFERENCES batches(id)
);

-- =====================================
-- CUSTOMERS
-- =====================================
CREATE TABLE customers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150),
    phone VARCHAR(20)
);

-- =====================================
-- SALES
-- =====================================
CREATE TABLE sales (
    id INT AUTO_INCREMENT PRIMARY KEY,
    bill_no VARCHAR(50),
    customer_name VARCHAR(150),
    total_amount DECIMAL(10,2),
    discount DECIMAL(10,2),
    gst DECIMAL(10,2),
    final_amount DECIMAL(10,2),
    payment_type ENUM('cash', 'credit'),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- =====================================
-- SALE ITEMS
-- =====================================
CREATE TABLE sale_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sale_id INT,
    batch_id INT,
    quantity INT,
    mrp DECIMAL(10,2),
    purchase_price DECIMAL(10,2),
    total DECIMAL(10,2),
    FOREIGN KEY (sale_id) REFERENCES sales(id),
    FOREIGN KEY (batch_id) REFERENCES batches(id)
);

-- =====================================
-- LEDGER
-- =====================================
CREATE TABLE ledger (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT,
    sale_id INT,
    debit DECIMAL(10,2) DEFAULT 0,
    credit DECIMAL(10,2) DEFAULT 0,
    balance DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (sale_id) REFERENCES sales(id)
);

-- =====================================
-- PAYMENTS
-- =====================================
CREATE TABLE payments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT,
    amount DECIMAL(10,2),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);

