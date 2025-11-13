// Main entry point for AdminLTE dashboard
import 'admin-lte';
import '../scss/main.scss';

// Initialize AdminLTE components
document.addEventListener('DOMContentLoaded', () => {
  console.log('PlantSight Dashboard Initialized');
  
  // Initialize tooltips
  const tooltips = document.querySelectorAll('[data-toggle="tooltip"]');
  tooltips.forEach((tooltip) => {
    // @ts-ignore - Bootstrap tooltip
    $(tooltip).tooltip();
  });
  
  // Initialize popovers
  const popovers = document.querySelectorAll('[data-toggle="popover"]');
  popovers.forEach((popover) => {
    // @ts-ignore - Bootstrap popover
    $(popover).popover();
  });
});

// Export utility functions
export function showToast(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info') {
  // @ts-ignore - toastr
  toastr[type](message);
}

export function showLoading() {
  const overlay = document.getElementById('loadingOverlay');
  if (overlay) {
    overlay.classList.remove('hidden');
  }
}

export function hideLoading() {
  const overlay = document.getElementById('loadingOverlay');
  if (overlay) {
    overlay.classList.add('hidden');
  }
}

export async function fetchAPI(url: string, options?: RequestInit) {
  try {
    const response = await fetch(url, options);
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'API request failed');
    }
    return await response.json();
  } catch (error) {
    console.error('API Error:', error);
    throw error;
  }
}

