import {
  BrowserRouter,
  Routes,
  Route,
} from 'react-router-dom';

import Header from './components/Header';
import Footer from './components/Footer';

import HomePage from './pages/HomePage';
import ServicesPage from './pages/ServicesPage';
import ProductsPage from './pages/ProductsPage';
import CartPage from './pages/CartPage';
import ProfilePage from './pages/ProfilePage';
import AdminPage from './pages/AdminPage';

import './styles/theme.css';

function App() {
  return (
    <BrowserRouter>
      <Header />

      <main style={{ minHeight: '60vh' }}>
        <Routes>
          <Route
            path="/"
            element={<HomePage />}
          />

          <Route
            path="/services"
            element={<ServicesPage />}
          />

          <Route
            path="/products"
            element={<ProductsPage />}
          />

          <Route
            path="/cart"
            element={<CartPage />}
          />

          <Route
            path="/profile"
            element={<ProfilePage />}
          />

          <Route
            path="/admin"
            element={<AdminPage />}
          />
        </Routes>
      </main>

      <Footer />
    </BrowserRouter>
  );
}

export default App;