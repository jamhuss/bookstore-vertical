import { useState } from 'react';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getApiErrorMessage, ordersApi } from '../api';
import type { Order } from '../types';
import { OrderModal } from '../components/OrderModal';

const pageSize = 5;

export function OrdersPage() {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);

    const [modalOpen, setModalOpen] = useState(false);
    const [editingOrder, setEditingOrder] = useState<Order | null>(null);

    const { data, isPending, error } = useQuery({
        queryKey: ['orders', page],
        queryFn: () => ordersApi.list(page, pageSize),
        placeholderData: keepPreviousData,
    });



    const deleteMutation = useMutation({
        mutationFn: (order: Order) => ordersApi.remove(order.id),
        onSuccess: () => {
            if (data && data.items.length === 1 && page > 1) {
                setPage(page - 1);
            } else {
                queryClient.invalidateQueries({ queryKey: ['orders'] });
            }
        },
    });

    function openCreate() {
        setEditingOrder(null);
        setModalOpen(true);
    }

    function openEdit(order: Order) {
        setEditingOrder(order);
        setModalOpen(true);
    }

    function handleSaved() {
        setModalOpen(false);
        queryClient.invalidateQueries({ queryKey: ['orders'] });
    }

    function remove(order: Order) {
        if (!confirm(`Delete order #${order.id}?`)) {
            return;
        }
        deleteMutation.mutate(order);
    }

    const errorMessage = error
        ? getApiErrorMessage(error)
        : deleteMutation.error
          ? getApiErrorMessage(deleteMutation.error)
          : null;

    return (
        <div>
            <div className="page-header">
                <h1>Orders</h1>
                <button
                    type="button"
                    className="primary"
                    onClick={openCreate}
                >
                    + New order
                </button>
            </div>

            {errorMessage && <p className="error">{errorMessage}</p>}
            {isPending && <p>Loading…</p>}

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
                    onClose={() => setModalOpen(false)}
                    onSaved={handleSaved}
                />
            )}
        </div>
    );
}
