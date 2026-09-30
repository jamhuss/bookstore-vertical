import { useEffect, useState } from 'react';
import { booksApi, getApiErrorMessage, ordersApi } from '../api';
import type { Book, Order, PagedResult } from '../types';
import { OrderModal } from '../components/OrderModal';

export function OrdersPage() {
    const [data, setData] = useState<PagedResult<Order> | null>(null);
    const [page, setPage] = useState(1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [books, setBooks] = useState<Book[]>([]);

    const [modalOpen, setModalOpen] = useState(false);
    const [editingOrder, setEditingOrder] = useState<Order | null>(null);

    const pageSize = 5;

    async function load() {
        setLoading(true);
        setError(null);
        try {
            setData(await ordersApi.list(page, pageSize));
        } catch (e) {
            setError(getApiErrorMessage(e));
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        let cancelled = false;
        (async () => {
            setLoading(true);
            setError(null);
            try {
                const result = await ordersApi.list(page, pageSize);
                if (!cancelled) {
                    setData(result);
                }
            } catch (e) {
                if (!cancelled) {
                    setError(getApiErrorMessage(e));
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        })();
        return () => {
            cancelled = true;
        };
    }, [page]);

    useEffect(() => {
        booksApi
            .list(1, 100)
            .then((r) => setBooks(r.items))
            .catch((e) => setError(getApiErrorMessage(e)));
    }, []);

    function openCreate() {
        setEditingOrder(null);
        setModalOpen(true);
    }

    function openEdit(order: Order) {
        setEditingOrder(order);
        setModalOpen(true);
    }

    async function handleSaved() {
        setModalOpen(false);
        await load();
    }

    async function remove(order: Order) {
        if (!confirm(`Delete order #${order.id}?`)) {
            return;
        }
        try {
            await ordersApi.remove(order.id);
            if (data && data.items.length === 1 && page > 1) {
                setPage(page - 1);
            } else {
                await load();
            }
        } catch (e) {
            setError(getApiErrorMessage(e));
        }
    }

    return (
        <div>
            <div className="page-header">
                <h1>Orders</h1>
                <button
                    type="button"
                    className="primary"
                    onClick={openCreate}
                    disabled={!books.length}
                >
                    + New order
                </button>
            </div>

            {error && <p className="error">{error}</p>}
            {loading && <p>Loading…</p>}

            {data && (
                <>
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th>Customer</th>
                                <th>Items</th>
                                <th className="num">Total</th>
                                <th className="actions">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {data.items.map((o) => (
                                <tr key={o.id}>
                                    <td>{o.id}</td>
                                    <td>{o.userEmail}</td>
                                    <td>
                                        <ul className="line-items">
                                            {o.items.map((i) => (
                                                <li key={i.bookId}>
                                                    {i.quantity} × {i.title}{' '}
                                                    <span className="muted">
                                                        ({i.unitPrice.toFixed(2)} kr)
                                                    </span>
                                                </li>
                                            ))}
                                        </ul>
                                    </td>
                                    <td className="num">{o.totalPrice.toFixed(2)} kr</td>
                                    <td className="actions">
                                        <button type="button" onClick={() => openEdit(o)}>
                                            Edit
                                        </button>
                                        <button
                                            type="button"
                                            className="danger"
                                            onClick={() => remove(o)}
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {data.items.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="empty">
                                        No orders yet.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>

                    <div className="pager">
                        <button
                            type="button"
                            disabled={!data.hasPrevious}
                            onClick={() => setPage((p) => p - 1)}
                        >
                            ‹ Prev
                        </button>
                        <span>
                            Page {data.page} of {data.totalPages || 1} · {data.totalCount} total
                        </span>
                        <button
                            type="button"
                            disabled={!data.hasNext}
                            onClick={() => setPage((p) => p + 1)}
                        >
                            Next ›
                        </button>
                    </div>
                </>
            )}

            {modalOpen && (
                <OrderModal
                    order={editingOrder}
                    books={books}
                    onClose={() => setModalOpen(false)}
                    onSaved={handleSaved}
                />
            )}
        </div>
    );
}
