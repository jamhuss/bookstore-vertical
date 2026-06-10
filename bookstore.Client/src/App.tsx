import { useState } from 'react';
import { BooksPage } from './pages/BooksPage';
import { OrdersPage } from './pages/OrdersPage';
import './App.css';

type Tab = 'books' | 'orders';

function App() {
    const [tab, setTab] = useState<Tab>('books');

    return (
        <div className="app">
            <header className="app-header">
                <div className="brand">BookStore</div>
                <nav className="tabs">
                    <button
                        type="button"
                        className={tab === 'books' ? 'active' : ''}
                        onClick={() => setTab('books')}
                    >
                        Books
                    </button>
                    <button
                        type="button"
                        className={tab === 'orders' ? 'active' : ''}
                        onClick={() => setTab('orders')}
                    >
                        Orders
                    </button>
                </nav>
            </header>

            <main className="app-main">
                {tab === 'books' ? <BooksPage /> : <OrdersPage />}
            </main>
        </div>
    );
}

export default App;
