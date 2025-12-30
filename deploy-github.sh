#!/bin/bash

# GitHub Repository Setup Script
echo "=== Employee Management System - GitHub Deployment ==="
echo ""

# Check if git is initialized
if [ ! -d ".git" ]; then
    echo "Initializing Git repository..."
    git init
fi

# Add all files
echo "Adding files to Git..."
git add .

# Commit changes
echo "Committing changes..."
git commit -m "Deploy Employee Management System to GitHub"

# Instructions for GitHub
echo ""
echo "=== NEXT STEPS ==="
echo "1. Go to https://github.com/new"
echo "2. Create repository named: employee-management-system"
echo "3. Don't initialize with README"
echo "4. Copy the repository URL"
echo "5. Run these commands:"
echo ""
echo "   git remote add origin https://github.com/YOUR_USERNAME/employee-management-system.git"
echo "   git branch -M main"
echo "   git push -u origin main"
echo ""
echo "Your code is ready for GitHub!"