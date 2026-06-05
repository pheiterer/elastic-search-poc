import { useState, useEffect, useCallback } from 'react'
import axios from 'axios'
import { debounce } from 'lodash'

interface Event {
  id: number
  name: string
}

function App() {
  const [events, setEvents] = useState<Event[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [loading, setLoading] = useState(false)

  const fetchEvents = async (query: string) => {
    setLoading(true)
    try {
      const url = query 
        ? `http://localhost:5000/api/events/search?q=${encodeURIComponent(query)}`
        : 'http://localhost:5000/api/events'
      
      const response = await axios.get<Event[]>(url)
      setEvents(response.data)
    } catch (error) {
      console.error('Error fetching events:', error)
    } finally {
      setLoading(false)
    }
  }

  // Create a debounced version of fetchEvents
  const debouncedFetch = useCallback(
    debounce((query: string) => fetchEvents(query), 300),
    []
  )

  useEffect(() => {
    // Initial fetch
    fetchEvents('')
  }, [])

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value
    setSearchTerm(value)
    debouncedFetch(value)
  }

  return (
    <div className="container">
      <h1>Event Search</h1>
      <div className="search-box">
        <input
          type="text"
          placeholder="Search events (e.g. Coldplay, Taylor)..."
          value={searchTerm}
          onChange={handleSearchChange}
          autoFocus
        />
        {loading && <span className="loader">Searching...</span>}
      </div>

      <div className="event-list">
        {events.length > 0 ? (
          events.map((event) => (
            <div key={event.id} className="event-card">
              <h3>{event.name}</h3>
              <p>ID: {event.id}</p>
            </div>
          ))
        ) : (
          <p>No events found.</p>
        )}
      </div>
    </div>
  )
}

export default App
